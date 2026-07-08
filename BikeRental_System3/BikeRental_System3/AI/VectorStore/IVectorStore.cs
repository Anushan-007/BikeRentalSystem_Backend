using BikeRental_System3.AI.Models;

namespace BikeRental_System3.AI.VectorStore
{
    /// <summary>
    /// Defines the contract for persisting and querying document embeddings
    /// in a vector-capable data store.
    /// </summary>
    /// <remarks>
    /// Implementations write to and read from the <c>document_vectors</c> table.
    /// Phase 4.5 will call <see cref="SearchSimilarAsync"/> to retrieve relevant
    /// chunks and inject them into the system prompt as context.
    /// </remarks>
    public interface IVectorStore
    {
        /// <summary>
        /// Persists a single <see cref="VectorDocument"/> to the store.
        /// </summary>
        /// <param name="document">The document chunk and its embedding to store.</param>
        /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a duplicate chunk (same DocumentId + ChunkIndex) already exists.
        /// </exception>
        Task SaveAsync(VectorDocument document, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists a collection of <see cref="VectorDocument"/> records in a single
        /// batch operation. Strongly preferred over repeated <see cref="SaveAsync"/>
        /// calls when storing large numbers of chunks.
        /// </summary>
        /// <param name="documents">The document chunks and their embeddings to store.</param>
        /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="documents"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when any chunk in the batch would create a duplicate entry.
        /// The entire batch is rolled back.
        /// </exception>
        Task SaveManyAsync(IEnumerable<VectorDocument> documents, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the top-<paramref name="topK"/> most semantically similar documents
        /// to the supplied query embedding, ranked by ascending cosine distance.
        /// </summary>
        /// <param name="queryEmbedding">
        /// The 1536-dimensional query vector produced by the embedding model.
        /// Must contain exactly 1536 elements.
        /// </param>
        /// <param name="topK">
        /// Maximum number of results to return. Must be a positive integer. Default: 5.
        /// </param>
        /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
        /// <returns>
        /// An ordered, read-only list of matching <see cref="VectorDocument"/> records
        /// (most similar first). May return fewer than <paramref name="topK"/> results
        /// if the store contains fewer documents.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="topK"/> is less than or equal to zero.
        /// </exception>
        Task<IReadOnlyList<VectorDocument>> SearchSimilarAsync(
            float[] queryEmbedding,
            int topK = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all stored chunks that belong to the specified document.
        /// </summary>
        /// <param name="documentId">
        /// The unique identifier of the source document whose chunks should be removed.
        /// </param>
        /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="documentId"/> is null or whitespace.
        /// </exception>
        Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes every row from the vector store.
        /// </summary>
        /// <remarks>
        /// This operation truncates the entire table and is irreversible.
        /// Use with caution in production environments.
        /// </remarks>
        /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
        Task DeleteAllAsync(CancellationToken cancellationToken = default);
    }
}
