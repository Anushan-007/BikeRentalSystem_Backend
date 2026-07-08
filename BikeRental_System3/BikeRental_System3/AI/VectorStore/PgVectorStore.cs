using System.Diagnostics;
using BikeRental_System3.AI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Pgvector;

namespace BikeRental_System3.AI.VectorStore
{
    /// <summary>
    /// PostgreSQL + pgvector implementation of <see cref="IVectorStore"/>.
    /// Stores chunk embeddings in a <c>vector(1536)</c> column and supports
    /// cosine-distance similarity search via the pgvector <c>&lt;=&gt;</c> operator.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Relies on an <see cref="NpgsqlDataSource"/> registered in DI with pgvector
    /// support enabled via <c>UseVector()</c>.  All I/O is fully async and
    /// supports cooperative cancellation via <see cref="CancellationToken"/>.
    /// </para>
    /// <para>
    /// Batch inserts use <see cref="NpgsqlBatch"/> to send all commands in a single
    /// server round-trip, which is significantly faster than N individual statements
    /// for large document collections.
    /// </para>
    /// </remarks>
    public sealed class PgVectorStore : IVectorStore
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly string _tableName;
        private readonly ILogger<PgVectorStore> _logger;

        /// <summary>
        /// The expected number of dimensions for every embedding vector.
        /// Must match the <c>vector(1536)</c> column definition in PostgreSQL.
        /// </summary>
        private const int ExpectedDimensions = 1536;

        /// <summary>
        /// Initialises a new instance of <see cref="PgVectorStore"/>.
        /// </summary>
        /// <param name="dataSource">
        /// Pooled Npgsql data source registered with pgvector support
        /// (<c>dataSourceBuilder.UseVector()</c>).
        /// </param>
        /// <param name="options">
        /// Vector database configuration bound from <c>appsettings.json</c>.
        /// </param>
        /// <param name="logger">Logger for diagnostics and performance metrics.</param>
        public PgVectorStore(
            NpgsqlDataSource dataSource,
            IOptions<VectorDatabaseOptions> options,
            ILogger<PgVectorStore> logger)
        {
            _dataSource = dataSource
                ?? throw new ArgumentNullException(nameof(dataSource));
            _tableName  = options?.Value.TableName
                ?? throw new ArgumentNullException(nameof(options));
            _logger     = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task SaveAsync(
            VectorDocument document,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(document);
            ValidateEmbedding(document.Embedding, document.DocumentId);

            var sw = Stopwatch.StartNew();

            try
            {
                await using var cmd = _dataSource.CreateCommand(BuildInsertSql());
                BindParameters(cmd.Parameters, document);

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                sw.Stop();
                _logger.LogInformation(
                    "Inserted chunk — DocumentId: {DocumentId}, ChunkIndex: {ChunkIndex}, ElapsedMs: {ElapsedMs}",
                    document.DocumentId, document.ChunkIndex, sw.ElapsedMilliseconds);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException(
                    $"Duplicate chunk detected: DocumentId={document.DocumentId}, " +
                    $"ChunkIndex={document.ChunkIndex}. " +
                    "Call DeleteDocumentAsync before re-inserting the document.", ex);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Database error while inserting chunk — DocumentId: {DocumentId}, ChunkIndex: {ChunkIndex}",
                    document.DocumentId, document.ChunkIndex);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task SaveManyAsync(
            IEnumerable<VectorDocument> documents,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(documents);

            var chunks = documents.ToList();
            if (chunks.Count == 0)
            {
                _logger.LogDebug("SaveManyAsync called with an empty collection — skipping.");
                return;
            }

            foreach (var doc in chunks)
                ValidateEmbedding(doc.Embedding, doc.DocumentId);

            var sw = Stopwatch.StartNew();

            await using var conn = await _dataSource
                .OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var tx = await conn
                .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // NpgsqlBatch sends all INSERT commands in a single round-trip to
                // PostgreSQL, providing a substantial throughput advantage over N
                // individual ExecuteNonQueryAsync calls when storing thousands of chunks.
                await using var npgsqlBatch = new NpgsqlBatch(conn, tx);

                foreach (var doc in chunks)
                {
                    var batchCmd = new NpgsqlBatchCommand(BuildInsertSql());
                    BindParameters(batchCmd.Parameters, doc);
                    npgsqlBatch.BatchCommands.Add(batchCmd);
                }

                await npgsqlBatch.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                sw.Stop();
                _logger.LogInformation(
                    "Batch inserted {Count} chunks in {ElapsedMs} ms.",
                    chunks.Count, sw.ElapsedMilliseconds);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException(
                    "One or more duplicate chunks detected — entire batch rolled back. " +
                    "Call DeleteDocumentAsync before re-inserting the document.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error during batch insert of {Count} chunks — rolling back transaction.",
                    chunks.Count);
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<VectorDocument>> SearchSimilarAsync(
            float[] queryEmbedding,
            int topK = 5,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(queryEmbedding);
            ValidateEmbedding(queryEmbedding, "queryEmbedding");

            if (topK <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(topK), topK, "topK must be a positive integer.");

            var sw = Stopwatch.StartNew();

            // The <=> operator computes cosine distance between two vectors.
            // ORDER BY ASC returns the most similar documents first (distance → 0).
            var sql = $"""
                SELECT id, document_id, document_title, file_name,
                       chunk_index, content, embedding, created_at
                FROM   {_tableName}
                ORDER BY embedding <=> @embedding
                LIMIT  @topK
                """;

            try
            {
                await using var cmd = _dataSource.CreateCommand(sql);
                cmd.Parameters.AddWithValue("@embedding", new Vector(queryEmbedding));
                cmd.Parameters.AddWithValue("@topK",      topK);

                var results = new List<VectorDocument>(topK);

                await using var reader =
                    await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    results.Add(MapRow(reader));

                sw.Stop();
                _logger.LogInformation(
                    "Similarity search returned {Count}/{TopK} results in {ElapsedMs} ms.",
                    results.Count, topK, sw.ElapsedMilliseconds);

                return results.AsReadOnly();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error during similarity search.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteDocumentAsync(
            string documentId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException(
                    "documentId cannot be null or whitespace.", nameof(documentId));

            try
            {
                await using var cmd = _dataSource.CreateCommand(
                    $"DELETE FROM {_tableName} WHERE document_id = @documentId");
                cmd.Parameters.AddWithValue("@documentId", documentId);

                var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Deleted {Count} chunk(s) for DocumentId: {DocumentId}.",
                    affected, documentId);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Database error while deleting chunks for DocumentId: {DocumentId}.", documentId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var cmd = _dataSource.CreateCommand($"TRUNCATE TABLE {_tableName}");
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogWarning(
                    "All rows truncated from table: {TableName}. This action is irreversible.",
                    _tableName);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Database error while truncating table: {TableName}.", _tableName);
                throw;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>Builds the parameterised INSERT SQL for the configured table.</summary>
        private string BuildInsertSql() =>
            $"""
             INSERT INTO {_tableName}
                 (id, document_id, document_title, file_name, chunk_index, content, embedding, created_at)
             VALUES
                 (@id, @documentId, @documentTitle, @fileName, @chunkIndex, @content, @embedding, @createdAt)
             """;

        /// <summary>
        /// Binds all <see cref="VectorDocument"/> fields to the supplied parameter collection.
        /// Works for both <see cref="NpgsqlCommand.Parameters"/> and
        /// <see cref="NpgsqlBatchCommand.Parameters"/> since both expose
        /// <see cref="NpgsqlParameterCollection"/>.
        /// </summary>
        private static void BindParameters(NpgsqlParameterCollection p, VectorDocument doc)
        {
            p.AddWithValue("@id",            doc.Id == Guid.Empty ? Guid.NewGuid() : doc.Id);
            p.AddWithValue("@documentId",    doc.DocumentId);
            p.AddWithValue("@documentTitle", doc.DocumentTitle);
            p.AddWithValue("@fileName",      doc.FileName);
            p.AddWithValue("@chunkIndex",    doc.ChunkIndex);
            p.AddWithValue("@content",       doc.Content);
            p.AddWithValue("@embedding",     new Vector(doc.Embedding));
            p.AddWithValue("@createdAt",     doc.CreatedAt == default
                                                 ? DateTime.UtcNow
                                                 : doc.CreatedAt);
        }

        /// <summary>
        /// Maps a single database row (in SELECT column order) to a <see cref="VectorDocument"/>.
        /// Column ordinals: 0=id, 1=document_id, 2=document_title, 3=file_name,
        /// 4=chunk_index, 5=content, 6=embedding, 7=created_at.
        /// </summary>
        private static VectorDocument MapRow(NpgsqlDataReader reader)
        {
            var vector = reader.GetFieldValue<Vector>(6);

            return new VectorDocument
            {
                Id            = reader.GetGuid(0),
                DocumentId    = reader.GetString(1),
                DocumentTitle = reader.GetString(2),
                FileName      = reader.GetString(3),
                ChunkIndex    = reader.GetInt32(4),
                Content       = reader.GetString(5),
                Embedding     = vector.Memory.ToArray(),
                CreatedAt     = reader.GetDateTime(7)
            };
        }

        /// <summary>
        /// Validates that an embedding array is non-empty and has exactly
        /// <see cref="ExpectedDimensions"/> elements.
        /// </summary>
        /// <param name="embedding">The embedding to validate.</param>
        /// <param name="contextName">Name used in the exception message for diagnostics.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the embedding is empty or has an unexpected dimension count.
        /// </exception>
        private static void ValidateEmbedding(float[] embedding, string contextName)
        {
            if (embedding.Length == 0)
                throw new ArgumentException(
                    $"Embedding for '{contextName}' is empty.", nameof(embedding));

            if (embedding.Length != ExpectedDimensions)
                throw new ArgumentException(
                    $"Embedding for '{contextName}' has {embedding.Length} dimension(s); " +
                    $"expected {ExpectedDimensions}. Verify the embedding model is " +
                    "'text-embedding-3-small' (1536 dimensions).", nameof(embedding));
        }
    }
}
