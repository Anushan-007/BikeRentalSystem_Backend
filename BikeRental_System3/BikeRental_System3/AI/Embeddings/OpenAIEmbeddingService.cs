#pragma warning disable SKEXP0001  // ITextEmbeddingGenerationService is [Experimental]

using BikeRental_System3.AI.Models;
using Microsoft.SemanticKernel.Embeddings;

namespace BikeRental_System3.AI.Embeddings
{
    /// <summary>
    /// Converts DocumentChunks into EmbeddedChunks using OpenAI text-embedding-3-small
    /// via Semantic Kernel's ITextEmbeddingGenerationService.
    ///
    /// API CALL (batch of N chunks = 1 HTTP request):
    ///   POST https://api.openai.com/v1/embeddings
    ///   { "model": "text-embedding-3-small", "input": ["chunk1 text", "chunk2 text", ...] }
    ///   → { "data": [{ "embedding": [0.023, -0.114, ...] }, ...] }
    ///
    /// BATCH DESIGN:
    ///   EmbedAsync(chunk)   → delegates to EmbedAllAsync([chunk]) → 1 API call
    ///   EmbedAllAsync(N)    → 1 API call for all N chunks
    ///   OpenAI limit: 2048 inputs per request (typical use: 10-50 chunks).
    ///
    /// Lifetime: Singleton — stateless, thread-safe.
    /// </summary>
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly ITextEmbeddingGenerationService _skEmbeddingService;
        private readonly string _modelId;
        private readonly ILogger<OpenAIEmbeddingService> _logger;

        public OpenAIEmbeddingService(
            ITextEmbeddingGenerationService skEmbeddingService,
            string modelId,
            ILogger<OpenAIEmbeddingService> logger)
        {
            _skEmbeddingService = skEmbeddingService;
            _modelId            = modelId;
            _logger             = logger;
        }

        public async Task<EmbeddedChunk> EmbedAsync(
            DocumentChunk chunk,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunk);
            var results = await EmbedAllAsync([chunk], cancellationToken);
            return results[0];
        }

        public async Task<IReadOnlyList<EmbeddedChunk>> EmbedAllAsync(
            IEnumerable<DocumentChunk> chunks,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunks);

            var chunkList = chunks.ToList();
            if (chunkList.Count == 0)
                return Array.Empty<EmbeddedChunk>();

            _logger.LogInformation(
                "Embedding {Count} chunk(s) using model '{Model}'.",
                chunkList.Count, _modelId);

            // Single batch API call — all chunk texts sent in one HTTP request
            var texts   = chunkList.Select(c => c.Content).ToList();
            var vectors = await _skEmbeddingService.GenerateEmbeddingsAsync(
                texts, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Received {Count} vector(s), {Dims} dimensions each.",
                vectors.Count, vectors.Count > 0 ? vectors[0].Length : 0);

            var embeddedAt = DateTime.UtcNow;

            return chunkList
                .Zip(vectors, (chunk, vector) => new EmbeddedChunk
                {
                    Chunk      = chunk,
                    Vector     = vector.ToArray(),
                    ModelId    = _modelId,
                    EmbeddedAt = embeddedAt
                })
                .ToList()
                .AsReadOnly();
        }
    }
}
