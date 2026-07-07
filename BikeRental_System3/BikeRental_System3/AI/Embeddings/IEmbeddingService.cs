using BikeRental_System3.AI.Models;

namespace BikeRental_System3.AI.Embeddings
{
    /// <summary>
    /// Contract for converting DocumentChunks into EmbeddedChunks (text → float[] vector).
    ///
    /// LangChain equivalent (Python):
    ///   from langchain.embeddings import OpenAIEmbeddings
    ///   embeddings = OpenAIEmbeddings(model="text-embedding-3-small")
    ///   vector = embeddings.embed_query("advance payment required?")
    ///
    /// SOLID — this interface is SK-agnostic:
    ///   No Semantic Kernel types appear here. Callers only see EmbeddedChunk (our domain type).
    ///   Swapping SK for another library only changes OpenAIEmbeddingService — no caller affected.
    ///
    /// Implementations:
    ///   OpenAIEmbeddingService → text-embedding-3-small via SK  (Phase 4.3) ← done
    ///   AzureEmbeddingService  → Azure OpenAI deployment          (future)
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Embeds a single chunk. For N chunks, prefer EmbedAllAsync (1 API call, not N).
        /// Throws: ArgumentNullException, HttpRequestException (OpenAI unreachable).
        /// </summary>
        Task<EmbeddedChunk> EmbedAsync(
            DocumentChunk chunk,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Embeds N chunks in a SINGLE batch API call.
        ///
        /// WHY BATCH:
        ///   N individual calls = N HTTP round-trips (slow, hits rate limits).
        ///   1 batch call       = 1 HTTP round-trip  (fast, same token cost).
        ///
        /// ORDER: result[i].Chunk == chunks[i] for all i.
        /// EMPTY: returns empty list, no API call made.
        /// THROWS: ArgumentNullException, HttpRequestException.
        /// </summary>
        Task<IReadOnlyList<EmbeddedChunk>> EmbedAllAsync(
            IEnumerable<DocumentChunk> chunks,
            CancellationToken cancellationToken = default);
    }
}
