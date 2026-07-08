using BikeRental_System3.AI.Chunking;
using BikeRental_System3.AI.VectorStore;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    /// <summary>
    /// HTTP API for the document ingestion pipeline.
    /// Triggers Load → Chunk → Embed → Save in a single request.
    /// </summary>
    /// <remarks>
    /// Typical workflow:
    /// <list type="number">
    ///   <item><description>
    ///     Place PDF files in the <c>AI/Documents/</c> folder.
    ///   </description></item>
    ///   <item><description>
    ///     Call <c>POST /api/ingestion/ingest-folder</c> (no body required).
    ///   </description></item>
    ///   <item><description>
    ///     Verify the result: <c>ChunksSaved > 0</c>.
    ///   </description></item>
    ///   <item><description>
    ///     Confirm in PostgreSQL:
    ///     <c>SELECT COUNT(*) FROM document_vectors;</c>
    ///   </description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class IngestionController : ControllerBase
    {
        private readonly IDocumentIngestionService _ingestionService;
        private readonly IVectorStore _vectorStore;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<IngestionController> _logger;

        /// <summary>
        /// Initialises a new instance of <see cref="IngestionController"/>.
        /// </summary>
        public IngestionController(
            IDocumentIngestionService ingestionService,
            IVectorStore vectorStore,
            IWebHostEnvironment env,
            ILogger<IngestionController> logger)
        {
            _ingestionService = ingestionService ?? throw new ArgumentNullException(nameof(ingestionService));
            _vectorStore      = vectorStore      ?? throw new ArgumentNullException(nameof(vectorStore));
            _env              = env              ?? throw new ArgumentNullException(nameof(env));
            _logger           = logger           ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── POST /api/ingestion/ingest-folder ────────────────────────────────

        /// <summary>
        /// Ingests all documents from a folder through the full pipeline and
        /// saves the resulting embeddings to PostgreSQL.
        /// </summary>
        /// <remarks>
        /// When <c>FolderPath</c> is omitted the default <c>AI/Documents/</c> folder
        /// inside the application content root is used.
        /// </remarks>
        /// <param name="request">Optional. Folder path and chunking overrides.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>
        /// 200 OK with <see cref="IngestionResult"/> on success.<br/>
        /// 400 Bad Request if no documents were found or an ingestion error occurred.
        /// </returns>
        [HttpPost("ingest-folder")]
        [ProducesResponseType(typeof(IngestionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IngestionResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IngestFolder(
            [FromBody] IngestFolderRequest? request,
            CancellationToken cancellationToken)
        {
            var folderPath = string.IsNullOrWhiteSpace(request?.FolderPath)
                ? Path.Combine(_env.ContentRootPath, "AI", "Documents")
                : request.FolderPath;

            var chunkingOptions = BuildChunkingOptions(request?.ChunkSize, request?.ChunkOverlap);

            _logger.LogInformation("Ingestion requested — folder: {FolderPath}", folderPath);

            var result = await _ingestionService
                .IngestFolderAsync(folderPath, chunkingOptions, cancellationToken)
                .ConfigureAwait(false);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // ── POST /api/ingestion/ingest-document ──────────────────────────────

        /// <summary>
        /// Ingests a single document through the full pipeline and saves the
        /// resulting embeddings to PostgreSQL.
        /// </summary>
        /// <param name="request">Required. Absolute path to the document file.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>
        /// 200 OK with <see cref="IngestionResult"/> on success.<br/>
        /// 400 Bad Request if the document has no content.
        /// </returns>
        [HttpPost("ingest-document")]
        [ProducesResponseType(typeof(IngestionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IngestionResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IngestDocument(
            [FromBody] IngestDocumentRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request?.FilePath))
                return BadRequest(new { error = "FilePath is required." });

            var chunkingOptions = BuildChunkingOptions(request.ChunkSize, request.ChunkOverlap);

            _logger.LogInformation("Ingestion requested — file: {FilePath}", request.FilePath);

            var result = await _ingestionService
                .IngestDocumentAsync(request.FilePath, chunkingOptions, cancellationToken)
                .ConfigureAwait(false);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // ── DELETE /api/ingestion/all ────────────────────────────────────────

        /// <summary>
        /// Removes all records from the vector store (TRUNCATE).
        /// Use before re-ingesting after document changes.
        /// </summary>
        /// <param name="cancellationToken">Request cancellation token.</param>
        /// <returns>200 OK with a confirmation message.</returns>
        [HttpDelete("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAll(CancellationToken cancellationToken)
        {
            await _vectorStore.DeleteAllAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogWarning("All vectors deleted via API.");

            return Ok(new { message = "All vector records deleted from document_vectors." });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static ChunkingOptions? BuildChunkingOptions(int? chunkSize, int? chunkOverlap)
        {
            if (chunkSize is null && chunkOverlap is null)
                return null; // service uses ChunkingOptions.Default

            return new ChunkingOptions
            {
                ChunkSize    = chunkSize    ?? ChunkingOptions.DefaultChunkSize,
                ChunkOverlap = chunkOverlap ?? ChunkingOptions.DefaultChunkOverlap
            };
        }
    }

    // ── Request DTOs ─────────────────────────────────────────────────────────

    /// <summary>Request body for <c>POST /api/ingestion/ingest-folder</c>.</summary>
    public sealed class IngestFolderRequest
    {
        /// <summary>
        /// Absolute path to the folder containing source documents.
        /// When omitted, the application's <c>AI/Documents/</c> folder is used.
        /// </summary>
        public string? FolderPath { get; set; }

        /// <summary>
        /// Optional override for chunk size in characters.
        /// Default: 512.
        /// </summary>
        public int? ChunkSize { get; set; }

        /// <summary>
        /// Optional override for chunk overlap in characters.
        /// Default: 64.
        /// </summary>
        public int? ChunkOverlap { get; set; }
    }

    /// <summary>Request body for <c>POST /api/ingestion/ingest-document</c>.</summary>
    public sealed class IngestDocumentRequest
    {
        /// <summary>Absolute path to the source document (required).</summary>
        public string? FilePath { get; set; }

        /// <summary>Optional override for chunk size in characters.</summary>
        public int? ChunkSize { get; set; }

        /// <summary>Optional override for chunk overlap in characters.</summary>
        public int? ChunkOverlap { get; set; }
    }
}
