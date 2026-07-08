using System.Diagnostics;
using BikeRental_System3.AI.Chunking;
using BikeRental_System3.AI.Documents;
using BikeRental_System3.AI.Embeddings;
using BikeRental_System3.AI.Models;
using Microsoft.Extensions.Logging;

namespace BikeRental_System3.AI.VectorStore
{
    /// <summary>
    /// Orchestrates the full RAG ingestion pipeline in sequence:
    /// <list type="number">
    ///   <item><description>Load documents via <see cref="IDocumentLoader"/>.</description></item>
    ///   <item><description>Split into chunks via <see cref="IDocumentChunker"/>.</description></item>
    ///   <item><description>Generate embeddings via <see cref="IEmbeddingService"/> (single batch API call).</description></item>
    ///   <item><description>Map <see cref="EmbeddedChunk"/> → <see cref="VectorDocument"/>.</description></item>
    ///   <item><description>Persist to PostgreSQL via <see cref="IVectorStore.SaveManyAsync"/>.</description></item>
    /// </list>
    /// </summary>
    public sealed class DocumentIngestionService : IDocumentIngestionService
    {
        private readonly IDocumentLoader _documentLoader;
        private readonly IDocumentChunker _documentChunker;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly ILogger<DocumentIngestionService> _logger;

        /// <summary>
        /// Initialises a new instance of <see cref="DocumentIngestionService"/>.
        /// </summary>
        public DocumentIngestionService(
            IDocumentLoader documentLoader,
            IDocumentChunker documentChunker,
            IEmbeddingService embeddingService,
            IVectorStore vectorStore,
            ILogger<DocumentIngestionService> logger)
        {
            _documentLoader  = documentLoader  ?? throw new ArgumentNullException(nameof(documentLoader));
            _documentChunker = documentChunker ?? throw new ArgumentNullException(nameof(documentChunker));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _vectorStore     = vectorStore     ?? throw new ArgumentNullException(nameof(vectorStore));
            _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IngestionResult> IngestFolderAsync(
            string folderPath,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("folderPath cannot be null or whitespace.", nameof(folderPath));

            var chunkingOptions = options ?? ChunkingOptions.Default;
            var sw = Stopwatch.StartNew();

            _logger.LogInformation(
                "Ingestion started — folder: {FolderPath}, ChunkSize: {ChunkSize}, Overlap: {Overlap}",
                folderPath, chunkingOptions.ChunkSize, chunkingOptions.ChunkOverlap);

            // ── Stage 1: Load ────────────────────────────────────────────────
            var allDocuments = await _documentLoader
                .LoadFolderAsync(folderPath)
                .ConfigureAwait(false);

            var documents = allDocuments.Where(d => d.HasContent).ToList();

            _logger.LogInformation(
                "Load complete — {Total} file(s) found, {Valid} with content.",
                allDocuments.Count, documents.Count);

            if (documents.Count == 0)
            {
                return BuildResult(
                    documentsLoaded: 0, chunksCreated: 0,
                    embeddingsGenerated: 0, chunksSaved: 0,
                    elapsed: sw.ElapsedMilliseconds,
                    error: "No documents with extractable content found in the specified folder.");
            }

            // ── Stage 2: Chunk ───────────────────────────────────────────────
            var chunks = _documentChunker
                .ChunkAll(documents, chunkingOptions)
                .Where(c => c.HasContent)
                .ToList();

            _logger.LogInformation("Chunk complete — {Count} chunks created.", chunks.Count);

            if (chunks.Count == 0)
            {
                return BuildResult(
                    documentsLoaded: documents.Count, chunksCreated: 0,
                    embeddingsGenerated: 0, chunksSaved: 0,
                    elapsed: sw.ElapsedMilliseconds,
                    error: "No chunks could be produced from the loaded documents.");
            }

            // ── Stage 3: Embed ───────────────────────────────────────────────
            var embeddedChunks = await _embeddingService
                .EmbedAllAsync(chunks, cancellationToken)
                .ConfigureAwait(false);

            var validEmbeddings = embeddedChunks.Where(e => e.HasVector).ToList();

            _logger.LogInformation(
                "Embed complete — {Count} embeddings generated.", validEmbeddings.Count);

            // ── Stage 4: Map + Save ──────────────────────────────────────────
            var vectorDocuments = validEmbeddings
                .Select(MapToVectorDocument)
                .ToList();

            await _vectorStore
                .SaveManyAsync(vectorDocuments, cancellationToken)
                .ConfigureAwait(false);

            sw.Stop();
            _logger.LogInformation(
                "Ingestion complete — {Saved} chunks saved to PostgreSQL in {ElapsedMs} ms.",
                vectorDocuments.Count, sw.ElapsedMilliseconds);

            return new IngestionResult
            {
                DocumentsLoaded     = documents.Count,
                ChunksCreated       = chunks.Count,
                EmbeddingsGenerated = validEmbeddings.Count,
                ChunksSaved         = vectorDocuments.Count,
                ElapsedMilliseconds = sw.ElapsedMilliseconds
            };
        }

        /// <inheritdoc />
        public async Task<IngestionResult> IngestDocumentAsync(
            string filePath,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath cannot be null or whitespace.", nameof(filePath));

            var chunkingOptions = options ?? ChunkingOptions.Default;
            var sw = Stopwatch.StartNew();

            _logger.LogInformation("Ingestion started — file: {FilePath}", filePath);

            // ── Stage 1: Load ────────────────────────────────────────────────
            var document = await _documentLoader
                .LoadAsync(filePath)
                .ConfigureAwait(false);

            if (!document.HasContent)
            {
                return BuildResult(
                    documentsLoaded: 0, chunksCreated: 0,
                    embeddingsGenerated: 0, chunksSaved: 0,
                    elapsed: sw.ElapsedMilliseconds,
                    error: $"Document '{Path.GetFileName(filePath)}' has no extractable text content.");
            }

            _logger.LogInformation(
                "Loaded '{Title}' — {Length} chars.", document.Title, document.ContentLength);

            // ── Stage 2: Chunk ───────────────────────────────────────────────
            var chunks = _documentChunker
                .Chunk(document, chunkingOptions)
                .Where(c => c.HasContent)
                .ToList();

            _logger.LogInformation("Chunk complete — {Count} chunks created.", chunks.Count);

            // ── Stage 3: Embed ───────────────────────────────────────────────
            var embeddedChunks = await _embeddingService
                .EmbedAllAsync(chunks, cancellationToken)
                .ConfigureAwait(false);

            var validEmbeddings = embeddedChunks.Where(e => e.HasVector).ToList();

            _logger.LogInformation(
                "Embed complete — {Count} embeddings generated.", validEmbeddings.Count);

            // ── Stage 4: Map + Save ──────────────────────────────────────────
            var vectorDocuments = validEmbeddings
                .Select(MapToVectorDocument)
                .ToList();

            await _vectorStore
                .SaveManyAsync(vectorDocuments, cancellationToken)
                .ConfigureAwait(false);

            sw.Stop();
            _logger.LogInformation(
                "Ingestion complete — {Saved} chunks saved to PostgreSQL in {ElapsedMs} ms.",
                vectorDocuments.Count, sw.ElapsedMilliseconds);

            return new IngestionResult
            {
                DocumentsLoaded     = 1,
                ChunksCreated       = chunks.Count,
                EmbeddingsGenerated = validEmbeddings.Count,
                ChunksSaved         = vectorDocuments.Count,
                ElapsedMilliseconds = sw.ElapsedMilliseconds
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Maps an <see cref="EmbeddedChunk"/> to a <see cref="VectorDocument"/> for storage.
        /// <para>
        /// Field mapping:
        /// EmbeddedChunk.Chunk.DocumentId    → VectorDocument.DocumentId<br/>
        /// EmbeddedChunk.Chunk.DocumentTitle → VectorDocument.DocumentTitle<br/>
        /// EmbeddedChunk.Chunk.Source        → VectorDocument.FileName (file name only)<br/>
        /// EmbeddedChunk.Chunk.ChunkIndex    → VectorDocument.ChunkIndex<br/>
        /// EmbeddedChunk.Chunk.Content       → VectorDocument.Content<br/>
        /// EmbeddedChunk.Vector              → VectorDocument.Embedding<br/>
        /// </para>
        /// </summary>
        private static VectorDocument MapToVectorDocument(EmbeddedChunk embeddedChunk) =>
            new VectorDocument
            {
                Id            = Guid.NewGuid(),
                DocumentId    = embeddedChunk.Chunk.DocumentId,
                DocumentTitle = embeddedChunk.Chunk.DocumentTitle,
                FileName      = Path.GetFileName(embeddedChunk.Chunk.Source),
                ChunkIndex    = embeddedChunk.Chunk.ChunkIndex,
                Content       = embeddedChunk.Chunk.Content,
                Embedding     = embeddedChunk.Vector,
                CreatedAt     = DateTime.UtcNow
            };

        private static IngestionResult BuildResult(
            int documentsLoaded, int chunksCreated,
            int embeddingsGenerated, int chunksSaved,
            long elapsed, string error) =>
            new IngestionResult
            {
                DocumentsLoaded     = documentsLoaded,
                ChunksCreated       = chunksCreated,
                EmbeddingsGenerated = embeddingsGenerated,
                ChunksSaved         = chunksSaved,
                ElapsedMilliseconds = elapsed,
                Errors              = new[] { error }
            };
    }
}
