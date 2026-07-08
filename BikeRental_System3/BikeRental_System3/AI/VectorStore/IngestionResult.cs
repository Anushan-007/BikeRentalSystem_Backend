namespace BikeRental_System3.AI.VectorStore
{
    /// <summary>
    /// Describes the outcome of a document ingestion run through the full
    /// Load → Chunk → Embed → Save pipeline.
    /// Returned by <see cref="IDocumentIngestionService"/> methods and surfaced
    /// through the HTTP API for verification.
    /// </summary>
    public sealed class IngestionResult
    {
        /// <summary>Number of source documents successfully loaded with non-empty content.</summary>
        public int DocumentsLoaded { get; init; }

        /// <summary>Total number of chunks produced across all loaded documents.</summary>
        public int ChunksCreated { get; init; }

        /// <summary>Number of chunks for which embeddings were generated.</summary>
        public int EmbeddingsGenerated { get; init; }

        /// <summary>Number of <see cref="VectorDocument"/> records persisted to PostgreSQL.</summary>
        public int ChunksSaved { get; init; }

        /// <summary>Wall-clock time for the entire pipeline in milliseconds.</summary>
        public long ElapsedMilliseconds { get; init; }

        /// <summary>
        /// Human-readable error messages if any stage encountered a problem.
        /// An empty list indicates a fully successful run.
        /// </summary>
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        /// <summary><see langword="true"/> when no errors were recorded.</summary>
        public bool IsSuccess => Errors.Count == 0;
    }
}
