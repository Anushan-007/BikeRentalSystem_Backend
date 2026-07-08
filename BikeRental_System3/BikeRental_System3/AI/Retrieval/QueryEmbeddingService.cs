#pragma warning disable SKEXP0001  // ITextEmbeddingGenerationService is [Experimental] in SK 1.x

using System.Diagnostics;
using Microsoft.SemanticKernel.Embeddings;

namespace BikeRental_System3.AI.Retrieval
{
    /// <summary>
    /// Generates vector embeddings for user queries using OpenAI text-embedding-3-small
    /// via Semantic Kernel's ITextEmbeddingGenerationService.
    ///
    /// SAME model as Phase 4.3 (OpenAIEmbeddingService) — critical for cosine similarity
    /// to work correctly. Ingestion and retrieval MUST use the same embedding model.
    ///
    /// Lifetime: Singleton — stateless. ITextEmbeddingGenerationService is thread-safe.
    /// </summary>
    public sealed class QueryEmbeddingService : IQueryEmbeddingService
    {
        private readonly ITextEmbeddingGenerationService _embeddingService;
        private readonly ILogger<QueryEmbeddingService> _logger;

        /// <summary>
        /// Initialises a new instance of <see cref="QueryEmbeddingService"/>.
        /// </summary>
        public QueryEmbeddingService(
            ITextEmbeddingGenerationService embeddingService,
            ILogger<QueryEmbeddingService> logger)
        {
            _embeddingService = embeddingService
                ?? throw new ArgumentNullException(nameof(embeddingService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<float[]> EmbedQueryAsync(
            string question,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException(
                    "Question cannot be null or whitespace.", nameof(question));

            var sw = Stopwatch.StartNew();

            _logger.LogInformation(
                "Generating query embedding. Query: '{Query}'",
                question.Length > 80 ? question[..80] + "..." : question);

            // SK GenerateEmbeddingAsync returns ReadOnlyMemory<float>
            // Single call — 1 HTTP round-trip to OpenAI embeddings endpoint
            var embedding = await _embeddingService
                .GenerateEmbeddingAsync(question, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            sw.Stop();

            var vector = embedding.ToArray();

            _logger.LogInformation(
                "Query embedding generated in {ElapsedMs} ms. Dimensions: {Dims}.",
                sw.ElapsedMilliseconds, vector.Length);

            return vector;
        }
    }
}

#pragma warning restore SKEXP0001
