using System.Diagnostics;
using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.AI.VectorStore;
using Microsoft.Extensions.Options;

namespace BikeRental_System3.AI.Retrieval
{
    /// <summary>
    /// Phase 4.5 — RAG Context Provider.
    ///
    /// Implements IContextProvider using PostgreSQL + pgvector cosine similarity search.
    /// Registered in DI as the IContextProvider — replaces CompositeContextProvider.
    ///
    /// LangChain equivalent (Python):
    ///   retriever = vectorstore.as_retriever(search_kwargs={"k": 5})
    ///   context   = retriever.invoke(user_question)
    ///
    /// Pipeline (executed per user message):
    ///
    ///   User Question (string)
    ///         │
    ///         ▼  [1] IQueryEmbeddingService.EmbedQueryAsync
    ///   Query Embedding (float[1536])
    ///         │
    ///         ▼  [2] IVectorStore.SearchSimilarAsync  (ORDER BY embedding &lt;=&gt; @q LIMIT TopK)
    ///   Top-K VectorDocuments
    ///         │
    ///         ▼  [3] IContextBuilder.Build
    ///   Context String (injected into {{context}} in the system prompt)
    ///         │
    ///         ▼
    ///   BikeRentalChatChain → GPT → Answer
    ///
    /// Error handling:
    ///   Embedding failure  → returns string.Empty (GPT answers from general knowledge)
    ///   DB search failure  → returns string.Empty (graceful degradation)
    ///   No chunks found    → returns string.Empty (logged as Warning)
    ///
    /// BikeRentalChatChain does NOT change — it calls IContextProvider.GetContextAsync
    /// without knowing which implementation is active. This is the Open/Closed Principle.
    ///
    /// Lifetime: Singleton — all dependencies (IQueryEmbeddingService, IVectorStore,
    /// IContextBuilder, IOptions) are themselves Singleton-safe.
    /// </summary>
    public sealed class VectorStoreContextProvider : IContextProvider
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly IQueryEmbeddingService _queryEmbeddingService;
        private readonly IVectorStore            _vectorStore;
        private readonly IContextBuilder          _contextBuilder;
        private readonly RetrievalOptions         _options;
        private readonly ILogger<VectorStoreContextProvider> _logger;

        // ── Constructor ───────────────────────────────────────────────────────

        public VectorStoreContextProvider(
            IQueryEmbeddingService              queryEmbeddingService,
            IVectorStore                         vectorStore,
            IContextBuilder                      contextBuilder,
            IOptions<RetrievalOptions>           options,
            ILogger<VectorStoreContextProvider>  logger)
        {
            _queryEmbeddingService = queryEmbeddingService
                ?? throw new ArgumentNullException(nameof(queryEmbeddingService));
            _vectorStore           = vectorStore
                ?? throw new ArgumentNullException(nameof(vectorStore));
            _contextBuilder        = contextBuilder
                ?? throw new ArgumentNullException(nameof(contextBuilder));
            _options               = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            _logger                = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── IContextProvider Implementation ───────────────────────────────────

        /// <summary>
        /// Retrieves semantically relevant document chunks for the given user query
        /// using cosine similarity search in PostgreSQL pgvector, then formats them
        /// as a context string for GPT injection.
        ///
        /// Returns string.Empty on any failure — the chat pipeline continues without
        /// retrieved context (graceful degradation: GPT answers from training data).
        /// </summary>
        public async Task<string> GetContextAsync(
            string userQuery,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userQuery))
            {
                _logger.LogWarning("VectorStoreContextProvider: empty query — skipping retrieval.");
                return string.Empty;
            }

            var totalSw = Stopwatch.StartNew();

            _logger.LogInformation(
                "RAG retrieval started. Query: '{Query}' | TopK: {TopK}",
                userQuery.Length > 80 ? userQuery[..80] + "..." : userQuery,
                _options.TopK);

            // ── Step 1: Generate query embedding ──────────────────────────────
            // Sends the user question to OpenAI embeddings endpoint → float[1536]
            // Uses SAME model (text-embedding-3-small) as document ingestion phase
            float[] queryEmbedding;
            try
            {
                var embedSw = Stopwatch.StartNew();

                queryEmbedding = await _queryEmbeddingService
                    .EmbedQueryAsync(userQuery, cancellationToken)
                    .ConfigureAwait(false);

                embedSw.Stop();
                _logger.LogInformation(
                    "Step 1 complete — query embedding in {Ms} ms ({Dims} dims).",
                    embedSw.ElapsedMilliseconds, queryEmbedding.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "VectorStoreContextProvider: embedding failed — returning empty context.");
                return string.Empty;
            }

            // ── Step 2: Cosine similarity search in PostgreSQL ─────────────────
            // Executes: SELECT ... FROM document_vectors
            //           ORDER BY embedding <=> @queryEmbedding
            //           LIMIT @topK
            IReadOnlyList<AI.Models.VectorDocument> chunks;
            try
            {
                var searchSw = Stopwatch.StartNew();

                chunks = await _vectorStore
                    .SearchSimilarAsync(queryEmbedding, _options.TopK, cancellationToken)
                    .ConfigureAwait(false);

                searchSw.Stop();
                _logger.LogInformation(
                    "Step 2 complete — similarity search in {Ms} ms. Retrieved {Count}/{TopK} chunks.",
                    searchSw.ElapsedMilliseconds, chunks.Count, _options.TopK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "VectorStoreContextProvider: similarity search failed — returning empty context.");
                return string.Empty;
            }

            // ── Step 3: Handle empty results ──────────────────────────────────
            if (chunks.Count == 0)
            {
                _logger.LogWarning(
                    "VectorStoreContextProvider: no chunks found for query. " +
                    "GPT will answer without retrieved context.");
                return string.Empty;
            }

            // ── Step 4: Build formatted context string ─────────────────────────
            var context = _contextBuilder.Build(chunks, _options.MaximumContextCharacters);

            totalSw.Stop();

            _logger.LogInformation(
                "RAG retrieval complete in {TotalMs} ms. " +
                "Context: {ContextLen} chars from {ChunkCount} chunk(s). " +
                "Injecting into system prompt.",
                totalSw.ElapsedMilliseconds, context.Length, chunks.Count);

            return context;
        }
    }
}
