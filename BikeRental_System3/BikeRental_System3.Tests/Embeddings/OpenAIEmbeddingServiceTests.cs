#pragma warning disable SKEXP0001  // ITextEmbeddingGenerationService is [Experimental]

using BikeRental_System3.AI.Embeddings;
using BikeRental_System3.AI.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Embeddings;
using Moq;

namespace BikeRental_System3.Tests.Embeddings
{
    /// <summary>
    /// 18 unit tests for OpenAIEmbeddingService.
    /// Uses Moq to mock ITextEmbeddingGenerationService — no real API calls.
    /// </summary>
    public class OpenAIEmbeddingServiceTests
    {
        private const string ModelId = "text-embedding-3-small";

        // ── helpers ──────────────────────────────────────────────────────────

        private static DocumentChunk MakeChunk(string content = "hello", string id = "chunk-0") =>
            new() { Id = id, DocumentId = "doc1", DocumentTitle = "T", Source = "s", ChunkIndex = 0, Content = content, StartOffset = 0, EndOffset = content.Length };

        /// <summary>
        /// Builds a mock that returns one fake 3-dim vector per input string.
        /// The vector values are derived from the string's hashcode so each
        /// distinct text produces a distinct (deterministic) vector.
        /// </summary>
        private static (Mock<ITextEmbeddingGenerationService> Mock, OpenAIEmbeddingService Service)
            BuildService(int dims = 3)
        {
            var mockSk = new Mock<ITextEmbeddingGenerationService>();

            mockSk
                .Setup(s => s.GenerateEmbeddingsAsync(
                    It.IsAny<IList<string>>(),
                    It.IsAny<Microsoft.SemanticKernel.Kernel?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IList<string> texts,
                               Microsoft.SemanticKernel.Kernel? _,
                               CancellationToken __) =>
                {
                    var vectors = texts.Select(t =>
                    {
                        var arr = new float[dims];
                        var hash = t.GetHashCode();
                        for (int i = 0; i < dims; i++) arr[i] = (hash + i) * 0.001f;
                        return new ReadOnlyMemory<float>(arr);
                    }).ToList().AsReadOnly();
                    return (IList<ReadOnlyMemory<float>>)vectors;
                });

            var service = new OpenAIEmbeddingService(
                mockSk.Object,
                ModelId,
                NullLogger<OpenAIEmbeddingService>.Instance);

            return (mockSk, service);
        }

        // ── 1. EmbedAsync returns an EmbeddedChunk ────────────────────────────

        [Fact]
        public async Task EmbedAsync_SingleChunk_ReturnsEmbeddedChunk()
        {
            var (_, svc) = BuildService();
            var result   = await svc.EmbedAsync(MakeChunk());
            Assert.NotNull(result);
        }

        // ── 2. Original chunk preserved in result ─────────────────────────────

        [Fact]
        public async Task EmbedAsync_Result_ChunkMatchesInput()
        {
            var (_, svc) = BuildService();
            var chunk    = MakeChunk("advance payment");
            var result   = await svc.EmbedAsync(chunk);
            Assert.Equal(chunk, result.Chunk);
        }

        // ── 3. ModelId is set on result ───────────────────────────────────────

        [Fact]
        public async Task EmbedAsync_Result_ModelIdSet()
        {
            var (_, svc) = BuildService();
            var result   = await svc.EmbedAsync(MakeChunk());
            Assert.Equal(ModelId, result.ModelId);
        }

        // ── 4. EmbeddedAt is recent UTC ───────────────────────────────────────

        [Fact]
        public async Task EmbedAsync_Result_EmbeddedAtIsRecentUtc()
        {
            var before   = DateTime.UtcNow.AddSeconds(-2);
            var (_, svc) = BuildService();
            var result   = await svc.EmbedAsync(MakeChunk());
            var after    = DateTime.UtcNow.AddSeconds(2);
            Assert.InRange(result.EmbeddedAt, before, after);
        }

        // ── 5. Vector populated with correct dimensions ───────────────────────

        [Fact]
        public async Task EmbedAsync_Result_VectorHasExpectedDimensions()
        {
            var (_, svc) = BuildService(dims: 5);
            var result   = await svc.EmbedAsync(MakeChunk());
            Assert.Equal(5, result.Vector.Length);
        }

        // ── 6. HasVector is true after embedding ──────────────────────────────

        [Fact]
        public async Task EmbedAsync_Result_HasVectorIsTrue()
        {
            var (_, svc) = BuildService();
            var result   = await svc.EmbedAsync(MakeChunk());
            Assert.True(result.HasVector);
        }

        // ── 7. Chunk content is passed to SK service ──────────────────────────

        [Fact]
        public async Task EmbedAsync_PassesChunkContentToSkService()
        {
            var (mock, svc) = BuildService();
            await svc.EmbedAsync(MakeChunk("bike rental policy"));
            mock.Verify(s => s.GenerateEmbeddingsAsync(
                It.Is<IList<string>>(l => l.Count == 1 && l[0] == "bike rental policy"),
                It.IsAny<Microsoft.SemanticKernel.Kernel?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ── 8. Null chunk throws ArgumentNullException ────────────────────────

        [Fact]
        public async Task EmbedAsync_NullChunk_ThrowsArgumentNullException()
        {
            var (_, svc) = BuildService();
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => svc.EmbedAsync(null!));
        }

        // ── 9. EmbedAllAsync — null list throws ───────────────────────────────

        [Fact]
        public async Task EmbedAllAsync_NullList_ThrowsArgumentNullException()
        {
            var (_, svc) = BuildService();
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => svc.EmbedAllAsync(null!));
        }

        // ── 10. EmbedAllAsync — empty list returns empty, no API call ─────────

        [Fact]
        public async Task EmbedAllAsync_EmptyList_ReturnsEmptyWithoutApiCall()
        {
            var (mock, svc) = BuildService();
            var result = await svc.EmbedAllAsync(Array.Empty<DocumentChunk>());
            Assert.Empty(result);
            mock.Verify(s => s.GenerateEmbeddingsAsync(
                It.IsAny<IList<string>>(),
                It.IsAny<Microsoft.SemanticKernel.Kernel?>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        // ── 11. EmbedAllAsync — 3 chunks = exactly 1 API call (batch) ─────────

        [Fact]
        public async Task EmbedAllAsync_ThreeChunks_OneApiCall()
        {
            var (mock, svc) = BuildService();
            var chunks = new[]
            {
                MakeChunk("chunk one",   "c1"),
                MakeChunk("chunk two",   "c2"),
                MakeChunk("chunk three", "c3")
            };
            await svc.EmbedAllAsync(chunks);
            mock.Verify(s => s.GenerateEmbeddingsAsync(
                It.IsAny<IList<string>>(),
                It.IsAny<Microsoft.SemanticKernel.Kernel?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ── 12. EmbedAllAsync — result count matches input count ──────────────

        [Fact]
        public async Task EmbedAllAsync_ThreeChunks_ReturnsThreeResults()
        {
            var (_, svc) = BuildService();
            var chunks   = Enumerable.Range(0, 3)
                .Select(i => MakeChunk($"text {i}", $"c{i}"))
                .ToArray();
            var result = await svc.EmbedAllAsync(chunks);
            Assert.Equal(3, result.Count);
        }

        // ── 13. EmbedAllAsync — input order preserved ─────────────────────────

        [Fact]
        public async Task EmbedAllAsync_OrderPreserved()
        {
            var (_, svc) = BuildService();
            var chunks   = new[]
            {
                MakeChunk("alpha", "c0"),
                MakeChunk("beta",  "c1"),
                MakeChunk("gamma", "c2")
            };
            var result = await svc.EmbedAllAsync(chunks);
            for (int i = 0; i < chunks.Length; i++)
                Assert.Equal(chunks[i], result[i].Chunk);
        }

        // ── 14. Each result has ModelId set ───────────────────────────────────

        [Fact]
        public async Task EmbedAllAsync_AllResults_HaveModelId()
        {
            var (_, svc) = BuildService();
            var chunks   = Enumerable.Range(0, 3).Select(i => MakeChunk($"t{i}", $"c{i}")).ToArray();
            var result   = await svc.EmbedAllAsync(chunks);
            Assert.All(result, r => Assert.Equal(ModelId, r.ModelId));
        }

        // ── 15. Each result HasVector = true ──────────────────────────────────

        [Fact]
        public async Task EmbedAllAsync_AllResults_HasVectorTrue()
        {
            var (_, svc) = BuildService();
            var chunks   = Enumerable.Range(0, 3).Select(i => MakeChunk($"t{i}", $"c{i}")).ToArray();
            var result   = await svc.EmbedAllAsync(chunks);
            Assert.All(result, r => Assert.True(r.HasVector));
        }

        // ── 16. Dimensions property matches vector length ─────────────────────

        [Fact]
        public async Task EmbedAllAsync_Dimensions_MatchesVectorLength()
        {
            var (_, svc) = BuildService(dims: 4);
            var result   = await svc.EmbedAllAsync(new[] { MakeChunk() });
            Assert.Equal(result[0].Vector.Length, result[0].Dimensions);
        }

        // ── 17. API exception propagates ─────────────────────────────────────

        [Fact]
        public async Task EmbedAsync_ApiException_Propagates()
        {
            var mockSk = new Mock<ITextEmbeddingGenerationService>();
            mockSk
                .Setup(s => s.GenerateEmbeddingsAsync(
                    It.IsAny<IList<string>>(),
                    It.IsAny<Microsoft.SemanticKernel.Kernel?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("OpenAI unreachable"));

            var svc = new OpenAIEmbeddingService(
                mockSk.Object, ModelId, NullLogger<OpenAIEmbeddingService>.Instance);

            await Assert.ThrowsAsync<HttpRequestException>(
                () => svc.EmbedAsync(MakeChunk()));
        }

        // ── 18. EmbedAsync delegates to EmbedAllAsync (single batch of 1) ─────

        [Fact]
        public async Task EmbedAsync_DelegatesToEmbedAllAsync_SingleBatch()
        {
            var (mock, svc) = BuildService();
            await svc.EmbedAsync(MakeChunk("hello world"));

            // Exactly 1 API call with exactly 1 text
            mock.Verify(s => s.GenerateEmbeddingsAsync(
                It.Is<IList<string>>(l => l.Count == 1),
                It.IsAny<Microsoft.SemanticKernel.Kernel?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
