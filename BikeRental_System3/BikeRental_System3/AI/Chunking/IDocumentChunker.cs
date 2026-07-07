using BikeRental_System3.AI.Models;

namespace BikeRental_System3.AI.Chunking
{
    /// <summary>
    /// Contract for splitting a loaded Document into smaller DocumentChunks.
    ///
    /// LangChain equivalent (Python):
    ///   from langchain.text_splitter import RecursiveCharacterTextSplitter
    ///   splitter = RecursiveCharacterTextSplitter(chunk_size=512, chunk_overlap=64)
    ///   chunks = splitter.split_documents(docs)
    ///
    /// SOLID — this interface knows ONLY about splitting text:
    ///   ❌ Embeddings   ❌ Vector DB   ❌ Semantic Kernel   ❌ GPT
    ///   ✅ Pure in-memory string operations only
    ///
    /// Implementations:
    ///   FixedSizeChunker  → fixed character count + overlap  (Phase 4.2) ← done
    ///   SentenceChunker   → split at sentence boundaries      (future)
    /// </summary>
    public interface IDocumentChunker
    {
        /// <summary>
        /// Splits one document into an ordered list of chunks.
        /// Returns empty list (not exception) when document has no content.
        /// </summary>
        IReadOnlyList<DocumentChunk> Chunk(Document document, ChunkingOptions options);

        /// <summary>Convenience: chunks with default options (ChunkSize=512, Overlap=64).</summary>
        IReadOnlyList<DocumentChunk> Chunk(Document document)
            => Chunk(document, ChunkingOptions.Default);

        /// <summary>
        /// Chunks all documents and returns a flat list.
        /// Order: [doc1 chunk0, doc1 chunk1, ..., doc2 chunk0, ...]
        /// Empty documents are skipped.
        /// </summary>
        IReadOnlyList<DocumentChunk> ChunkAll(IEnumerable<Document> documents, ChunkingOptions options);

        /// <summary>Convenience: ChunkAll with default options.</summary>
        IReadOnlyList<DocumentChunk> ChunkAll(IEnumerable<Document> documents)
            => ChunkAll(documents, ChunkingOptions.Default);
    }
}
