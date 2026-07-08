namespace BikeRental_System3.AI.Retrieval
{
    /// <summary>
    /// Configuration options for the semantic retrieval pipeline (Phase 4.5).
    /// Bound from appsettings.json "Retrieval" section via IOptions&lt;RetrievalOptions&gt;.
    ///
    /// These values control how many chunks are retrieved from PostgreSQL and
    /// how the context string is truncated before injection into the system prompt.
    ///
    /// No magic numbers anywhere in the retrieval pipeline — all tuning happens here.
    /// </summary>
    public sealed class RetrievalOptions
    {
        /// <summary>Key name for the appsettings.json section.</summary>
        public const string SectionName = "Retrieval";

        /// <summary>
        /// Maximum number of most-similar chunks to retrieve per query.
        /// Default: 5.
        ///
        /// Trade-off:
        ///   Higher → more context, higher GPT token cost, risk of irrelevant info.
        ///   Lower  → less context, cheaper, but may miss relevant chunks.
        /// </summary>
        public int TopK { get; set; } = 5;

        /// <summary>
        /// Maximum total character count for the combined context string.
        /// Chunks are appended in similarity order; once this limit is reached
        /// no more chunks are added. Default: 4000 chars (~1000 GPT tokens).
        ///
        /// Prevents exceeding the LLM context window and controls token cost.
        /// </summary>
        public int MaximumContextCharacters { get; set; } = 4000;

        /// <summary>
        /// Minimum cosine similarity score (0.0–1.0) required for a chunk to be
        /// included in the context. Chunks below this threshold are discarded.
        /// Default: 0.0 (no filtering — all Top-K results are included).
        ///
        /// Note: pgvector <=> returns cosine DISTANCE (0=identical, 2=opposite).
        /// This option uses similarity (1 - distance), so 0.7 means distance &lt; 0.3.
        /// Reserved for future filtering — currently not applied in PgVectorStore.
        /// </summary>
        public double MinimumSimilarity { get; set; } = 0.0;
    }
}
