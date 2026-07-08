using BikeRental_System3.AI.Models;

namespace BikeRental_System3.AI.Retrieval
{
    /// <summary>
    /// Formats a list of retrieved VectorDocuments into a single context string
    /// for injection into the GPT system prompt (the {{context}} placeholder).
    ///
    /// RAG Pipeline position:
    ///   [VECTOR SEARCH]  → Top-K VectorDocuments
    ///   [BUILD CONTEXT]  ← you are here (Phase 4.5)
    ///   [INJECT PROMPT]  → {{context}} → GPT
    ///
    /// Single Responsibility:
    ///   This interface ONLY handles text formatting.
    ///   No GPT calls. No database access. No embeddings.
    ///
    /// Implementations:
    ///   ContextBuilder → formats chunks as Document/Chunk sections  (Phase 4.5) ← done
    /// </summary>
    public interface IContextBuilder
    {
        /// <summary>
        /// Builds a formatted context string from the retrieved document chunks.
        ///
        /// Returns string.Empty when:
        ///   - <paramref name="documents"/> is null or empty
        ///   - all document contents are whitespace
        ///
        /// Truncates at <paramref name="maxCharacters"/> to prevent context overflow.
        /// Chunks are included in the order provided (most-similar-first from SearchSimilarAsync).
        /// </summary>
        /// <param name="documents">
        /// The top-K chunks returned by the vector similarity search.
        /// Order is preserved in the output (most relevant first).
        /// </param>
        /// <param name="maxCharacters">
        /// Maximum total characters in the returned string.
        /// Defaults to 4000 (~1000 GPT tokens). Override via RetrievalOptions.
        /// </param>
        /// <returns>
        /// Formatted multi-section context string, or string.Empty if no content.
        /// </returns>
        string Build(IReadOnlyList<VectorDocument> documents, int maxCharacters = 4000);
    }
}
