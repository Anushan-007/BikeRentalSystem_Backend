namespace BikeRental_System3.AI.Retrieval
{
    /// <summary>
    /// Generates a vector embedding for a user's natural-language query.
    ///
    /// RAG Pipeline position:
    ///   [USER QUESTION]     → string
    ///   [EMBED QUERY]   ← you are here (Phase 4.5)
    ///   [VECTOR SEARCH]     → Top-K VectorDocuments
    ///   [BUILD CONTEXT]     → string
    ///   [INJECT PROMPT]     → GPT
    ///
    /// Why separate from IEmbeddingService?
    ///   IEmbeddingService (Phase 4.3) operates on DocumentChunks for ingestion.
    ///   A user query is NOT a DocumentChunk — it has no DocumentId, no ChunkIndex,
    ///   and no chunk overhead. Forcing a query through IEmbeddingService would
    ///   require creating a fake DocumentChunk, violating semantic clarity.
    ///   Single Responsibility: ingestion embedding ≠ query embedding.
    ///
    /// Implementations:
    ///   QueryEmbeddingService → SK ITextEmbeddingGenerationService  (Phase 4.5) ← done
    /// </summary>
    public interface IQueryEmbeddingService
    {
        /// <summary>
        /// Converts the user's question into a 1536-dimensional float vector using
        /// the same OpenAI text-embedding-3-small model used during document ingestion.
        ///
        /// Using the SAME model for both ingestion and query embedding is critical —
        /// vectors from different models are not comparable.
        /// </summary>
        /// <param name="question">
        /// The user's natural-language query. Must not be null or whitespace.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates cancellation to the OpenAI embedding API call.
        /// </param>
        /// <returns>
        /// A 1536-element float array representing the semantic meaning of the query.
        /// Passed directly to IVectorStore.SearchSimilarAsync.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="question"/> is null or whitespace.
        /// </exception>
        Task<float[]> EmbedQueryAsync(
            string question,
            CancellationToken cancellationToken = default);
    }
}
