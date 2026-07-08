namespace BikeRental_System3.AI.VectorStore
{
    /// <summary>
    /// Strongly-typed configuration for the PostgreSQL + pgvector database.
    /// Bound from the <c>"VectorDatabase"</c> section in <c>appsettings.json</c>.
    /// </summary>
    /// <remarks>
    /// Register with:
    /// <code>
    /// builder.Services.Configure&lt;VectorDatabaseOptions&gt;(
    ///     builder.Configuration.GetSection("VectorDatabase"));
    /// </code>
    /// </remarks>
    public sealed class VectorDatabaseOptions
    {
        /// <summary>
        /// Gets or sets the PostgreSQL server hostname or IP address.
        /// Default: <c>localhost</c>.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the PostgreSQL server port number.
        /// Default: <c>5432</c>.
        /// </summary>
        public int Port { get; set; } = 5432;

        /// <summary>
        /// Gets or sets the name of the PostgreSQL database that holds the
        /// <c>document_vectors</c> table.
        /// </summary>
        public string Database { get; set; } = "bike_rental_vectors";

        /// <summary>Gets or sets the PostgreSQL login username.</summary>
        public string Username { get; set; } = "postgres";

        /// <summary>Gets or sets the PostgreSQL login password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the table used to store vector documents.
        /// Default: <c>document_vectors</c>.
        /// </summary>
        public string TableName { get; set; } = "document_vectors";

        /// <summary>
        /// Builds an Npgsql-compatible connection string from the configured properties.
        /// </summary>
        /// <returns>A connection string suitable for <c>NpgsqlDataSourceBuilder</c>.</returns>
        public string BuildConnectionString() =>
            $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
    }
}
