using BikeRental_System3.AI.Documents;
using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.AI.Models;
using System.Text;

namespace BikeRental_System3.AI.Services
{
    /// <summary>
    /// Loads FAQ.pdf and Terms.pdf from AI/Documents/ at first call, caches the
    /// extracted text, and returns it as a context block for GPT.
    ///
    /// WHY PDF FOR THESE DOCUMENTS (vs database):
    ///   FAQ and rental terms are authored documents — they change rarely and are
    ///   written as structured prose. A PDF is the natural format.
    ///   Bike inventory (prices, availability) changes with every rental, so that
    ///   belongs in the database (handled by DatabaseContextProvider).
    ///
    /// CACHING:
    ///   PDFs are loaded once on the first request and the extracted text is held
    ///   in _cachedContent for the lifetime of the application.
    ///   If the PDF files are updated, the server must be restarted to pick up
    ///   the new content. This is acceptable for policy documents.
    ///
    /// THREAD SAFETY:
    ///   SemaphoreSlim(1,1) ensures only one thread loads the PDFs even if
    ///   multiple requests arrive simultaneously before the cache is populated.
    ///   Double-check after acquiring the lock avoids duplicate work.
    ///
    /// Lifetime: Singleton — safe because IDocumentLoader and IWebHostEnvironment
    /// are both Singleton-safe, and the cached string is immutable once set.
    /// </summary>
    public class PdfContextProvider : IContextProvider
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly IDocumentLoader _loader;
        private readonly string _documentsFolder;
        private readonly ILogger<PdfContextProvider> _logger;

        // ── Cache ─────────────────────────────────────────────────────────────

        // null  = not yet loaded
        // ""    = loaded but no PDF content found
        // text  = formatted context from the PDFs
        private string? _cachedContent;

        // One-at-a-time initialisation lock (double-checked locking pattern)
        private readonly SemaphoreSlim _initLock = new(1, 1);

        // ── Constructor ───────────────────────────────────────────────────────

        public PdfContextProvider(
            IDocumentLoader loader,
            IWebHostEnvironment env,
            ILogger<PdfContextProvider> logger)
        {
            _loader          = loader;
            _documentsFolder = Path.Combine(env.ContentRootPath, "AI", "Documents");
            _logger          = logger;

            _logger.LogInformation(
                "PdfContextProvider: documents folder = '{Folder}'", _documentsFolder);
        }

        // ── IContextProvider Implementation ───────────────────────────────────

        /// <summary>
        /// Returns cached PDF text on subsequent calls (zero I/O overhead).
        /// Loads PDFs on the first call using double-checked locking.
        /// </summary>
        public async Task<string> GetContextAsync(
            string userQuery,
            CancellationToken cancellationToken = default)
        {
            // Fast path — cache is already populated
            if (_cachedContent is not null)
                return _cachedContent;

            // Slow path — first request: load and cache the PDFs
            await _initLock.WaitAsync(cancellationToken);
            try
            {
                // Double-check: another thread may have populated cache while we waited
                if (_cachedContent is not null)
                    return _cachedContent;

                _logger.LogInformation(
                    "PdfContextProvider: loading documents from '{Folder}'", _documentsFolder);

                var docs = await _loader.LoadFolderAsync(_documentsFolder, cancellationToken);
                _cachedContent = FormatDocumentContext(docs);

                _logger.LogInformation(
                    "PdfContextProvider: loaded {Count} document(s), {Chars} chars cached.",
                    docs.Count, _cachedContent.Length);

                return _cachedContent;
            }
            catch (Exception ex)
            {
                // Graceful fallback: PDF load failed → empty context.
                // Chat still works; GPT answers from training knowledge for FAQ queries.
                _logger.LogError(ex,
                    "PdfContextProvider: failed to load documents. " +
                    "Chat will continue without PDF context.");

                _cachedContent = string.Empty;
                return _cachedContent;
            }
            finally
            {
                _initLock.Release();
            }
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Formats all loaded documents into a single labelled context block.
        ///
        /// Example output:
        ///   === Heaven Bike Rental — Policy & FAQ Documents ===
        ///
        ///   --- FAQ ---
        ///   Q1: What are your opening hours?
        ///   A: We are open 7 days a week, 8 AM to 8 PM.
        ///   ...
        ///
        ///   --- Terms ---
        ///   1. RENTAL AGREEMENT
        ///   The renter agrees to ...
        /// </summary>
        private static string FormatDocumentContext(IReadOnlyList<Document> docs)
        {
            var validDocs = docs.Where(d => d.HasContent).ToList();

            if (validDocs.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("=== Heaven Bike Rental — Policy & FAQ Documents ===");
            sb.AppendLine();

            foreach (var doc in validDocs)
            {
                sb.AppendLine($"--- {doc.Title} ---");
                sb.AppendLine(doc.Content.Trim());
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
