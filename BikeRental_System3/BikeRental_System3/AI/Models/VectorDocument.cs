namespace BikeRental_System3.AI.Models
{
    /// <summary>
    /// Represents a stored document chunk paired with its embedding vector.
    /// Persisted in the <c>document_vectors</c> PostgreSQL table via pgvector.
    /// </summary>
    /// <remarks>
    /// One <see cref="VectorDocument"/> corresponds to one <see cref="DocumentChunk"/>
    /// that has been embedded. The <see cref="Embedding"/> array is stored as a
    /// <c>vector(1536)</c> column and queried using cosine-distance operators.
    /// </remarks>
    public sealed class VectorDocument
    {
        /// <summary>Gets or sets the unique row identifier (UUID primary key).</summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the source document.
        /// Matches <see cref="DocumentChunk.DocumentId"/>.
        /// </summary>
        public string DocumentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable title of the source document
        /// (e.g., "FAQ", "Rental Terms").
        /// </summary>
        public string DocumentTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the original file name of the source document
        /// (e.g., "faq.pdf").
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the zero-based sequential index of this chunk within the document.
        /// Matches <see cref="DocumentChunk.ChunkIndex"/>.
        /// </summary>
        public int ChunkIndex { get; set; }

        /// <summary>Gets or sets the raw text content of this chunk.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the 1536-dimensional embedding vector produced by
        /// <c>text-embedding-3-small</c>.
        /// Stored as <c>vector(1536)</c> in PostgreSQL via the pgvector extension.
        /// </summary>
        public float[] Embedding { get; set; } = Array.Empty<float>();

        /// <summary>
        /// Gets or sets the UTC timestamp at which this record was inserted
        /// into the vector store.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
