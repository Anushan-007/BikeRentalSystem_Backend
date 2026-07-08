using BikeRental_System3.AI.Chunking;

namespace BikeRental_System3.AI.VectorStore
{
    /// <summary>
    /// Orchestrates the full RAG ingestion pipeline:
    /// Load → Chunk → Embed → Save.
    /// </summary>
    /// <remarks>
    /// One call to <see cref="IngestFolderAsync"/> or <see cref="IngestDocumentAsync"/>
    /// executes every stage and returns an <see cref="IngestionResult"/> that shows
    /// how many documents were loaded, chunks created, embeddings generated, and
    /// records saved to the vector store.
    /// </remarks>
    public interface IDocumentIngestionService
    {
        /// <summary>
        /// Loads every supported document from <paramref name="folderPath"/>,
        /// chunks, embeds, and persists all chunks to the vector store.
        /// </summary>
        /// <param name="folderPath">
        /// Absolute path to the folder containing source documents (PDFs, etc.).
        /// </param>
        /// <param name="options">
        /// Optional chunking configuration. When <see langword="null"/> the default
        /// profile is used (ChunkSize=512, ChunkOverlap=64).
        /// </param>
        /// <param name="cancellationToken">Propagates cancellation to all async stages.</param>
        /// <returns>A summary of the ingestion run.</returns>
        Task<IngestionResult> IngestFolderAsync(
            string folderPath,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads a single document at <paramref name="filePath"/>,
        /// chunks, embeds, and persists all chunks to the vector store.
        /// </summary>
        /// <param name="filePath">Absolute path to the source document.</param>
        /// <param name="options">
        /// Optional chunking configuration. When <see langword="null"/> the default
        /// profile is used (ChunkSize=512, ChunkOverlap=64).
        /// </param>
        /// <param name="cancellationToken">Propagates cancellation to all async stages.</param>
        /// <returns>A summary of the ingestion run.</returns>
        Task<IngestionResult> IngestDocumentAsync(
            string filePath,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}
