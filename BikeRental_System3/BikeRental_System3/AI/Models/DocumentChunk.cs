namespace BikeRental_System3.AI.Models
{
    /// <summary>
    /// Represents a single chunk produced by splitting a Document.
    ///
    /// RAG Pipeline position:
    ///   [LOAD]     → Document          (Phase 4.1)
    ///   [CHUNK]    → DocumentChunk  ← you are here (Phase 4.2)
    ///   [EMBED]    → float[] vector   (Phase 4.3)
    ///   [STORE]    → VectorDB record  (Phase 4.4)
    ///   [RETRIEVE] → matching chunks  (Phase 4.5)
    ///   [INJECT]   → {{context}}      (Phase 4.6)
    ///
    /// WHY SPLIT INTO CHUNKS?
    ///   A full PDF may be 20,000+ characters. Sending all of it as context every
    ///   turn hits the token limit and inflates cost. Chunking + retrieval (Phase 4.5)
    ///   ensures only the 2-3 most relevant chunks are injected per question.
    ///
    /// WHY OVERLAP?
    ///   A sentence near a chunk boundary may be split across two chunks.
    ///   Overlap duplicates boundary text in both chunks so the sentence is
    ///   always fully present in at least one chunk, improving retrieval accuracy.
    /// </summary>
    public class DocumentChunk
    {
        /// <summary>
        /// Globally unique chunk identifier: "{DocumentId}-chunk-{ChunkIndex}".
        /// Used as the Vector DB record key (upsert semantics on re-index).
        /// </summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>The Id of the Document this chunk was produced from.</summary>
        public string DocumentId { get; init; } = string.Empty;

        /// <summary>
        /// Human-readable document name, e.g. "FAQ" or "Terms".
        /// Used by GPT for attribution: "According to [FAQ], 30% advance is required."
        /// </summary>
        public string DocumentTitle { get; init; } = string.Empty;

        /// <summary>
        /// Absolute file path of the source document.
        /// Used for stale-chunk detection (file modified after EmbeddedAt → re-index).
        /// </summary>
        public string Source { get; init; } = string.Empty;

        /// <summary>Zero-based position of this chunk within its document.</summary>
        public int ChunkIndex { get; init; }

        /// <summary>
        /// Character position in Document.Content where this chunk starts (inclusive).
        /// </summary>
        public int StartOffset { get; init; }

        /// <summary>
        /// Character position in Document.Content where this chunk ends (exclusive).
        /// EndOffset - StartOffset == Content.Length always.
        /// </summary>
        public int EndOffset { get; init; }

        /// <summary>
        /// The plain-text content of this chunk (at most ChunkSize characters).
        /// Passed to the embedding API in Phase 4.3.
        /// </summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>Number of characters in Content.</summary>
        public int ContentLength => Content.Length;

        /// <summary>True if Content contains non-whitespace text.</summary>
        public bool HasContent => !string.IsNullOrWhiteSpace(Content);
    }
}
