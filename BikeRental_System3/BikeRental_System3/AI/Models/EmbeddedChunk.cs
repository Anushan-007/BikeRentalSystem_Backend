namespace BikeRental_System3.AI.Models
{
    /// <summary>
    /// A DocumentChunk paired with its float[] embedding vector.
    ///
    /// RAG Pipeline position:
    ///   [LOAD]     → Document          (Phase 4.1)
    ///   [CHUNK]    → DocumentChunk     (Phase 4.2)
    ///   [EMBED]    → EmbeddedChunk  ← you are here (Phase 4.3)
    ///   [STORE]    → VectorDB record   (Phase 4.4)
    ///   [RETRIEVE] → matching chunks   (Phase 4.5)
    ///   [INJECT]   → {{context}}       (Phase 4.6)
    ///
    /// WHAT IS AN EMBEDDING?
    ///   A list of floats that represents the MEANING of text in high-dimensional
    ///   space. Similar meaning → similar vector → close in space.
    ///   text-embedding-3-small produces 1 536-dimensional vectors.
    ///
    /// WHY CHAT MODEL ≠ EMBEDDING MODEL:
    ///   GPT-4o-mini   → generates next token (text output, $0.15/M tokens)
    ///   text-embedding-3-small → maps text to float[] (math output, $0.02/M tokens)
    ///   They cannot be swapped — different architecture, different training, different output.
    ///
    /// WHY EMBEDDINGS BEFORE VECTOR SEARCH:
    ///   Vector search computes cosine_similarity(queryVector, chunkVector).
    ///   Without embeddings there are only raw strings → only keyword search possible.
    ///   Embeddings enable SEMANTIC search (finds meaning, not just matching words).
    /// </summary>
    public class EmbeddedChunk
    {
        /// <summary>The original DocumentChunk that was embedded.</summary>
        public DocumentChunk Chunk { get; init; } = null!;

        /// <summary>
        /// The embedding vector (1 536 floats for text-embedding-3-small).
        /// Stored in Vector DB in Phase 4.4. Compared via cosine similarity in Phase 4.5.
        /// </summary>
        public float[] Vector { get; init; } = Array.Empty<float>();

        /// <summary>
        /// The embedding model that produced Vector (e.g. "text-embedding-3-small").
        /// Vectors from different models must NOT be mixed in the same Vector DB.
        /// </summary>
        public string ModelId { get; init; } = string.Empty;

        /// <summary>UTC timestamp when this embedding was generated.</summary>
        public DateTime EmbeddedAt { get; init; } = DateTime.UtcNow;

        /// <summary>Number of floats in Vector (the dimensionality).</summary>
        public int Dimensions => Vector.Length;

        /// <summary>True if Vector has been populated.</summary>
        public bool HasVector => Vector.Length > 0;
    }
}
