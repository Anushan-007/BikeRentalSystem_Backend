using BikeRental_System3.AI.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace BikeRental_System3.AI.Documents
{
    /// <summary>
    /// Loads PDF files from the file system and extracts plain text using iText7.
    ///
    /// LangChain equivalent (Python):
    ///   from langchain.document_loaders import PyPDFLoader
    ///   loader = PyPDFLoader("rental-policy.pdf")
    ///   docs = loader.load()
    ///
    /// Library: iText7 (AGPL — free for open-source / educational projects)
    ///   NuGet: itext7
    ///
    /// How iText7 extracts text:
    ///   A PDF contains binary drawing commands and encoded character data.
    ///   iText7's PdfTextExtractor reads these commands and uses
    ///   LocationTextExtractionStrategy to reconstruct reading order:
    ///     — Groups glyphs into words by proximity
    ///     — Sorts words left-to-right, top-to-bottom
    ///     — Returns clean plain text per page
    ///
    /// What we get:
    ///   ✓ Plain text words in reading order
    ///   ✓ Multi-column layouts (iText7 handles better than simple extraction)
    ///   ✗ Images / scanned PDFs (no text → empty page, no OCR)
    ///   ✗ Tables (structure is flattened to plain text)
    ///
    /// Limitations:
    ///   - Password-protected PDFs throw an exception (gracefully caught)
    ///   - Image-only PDFs produce empty content (no OCR capability)
    ///
    /// Lifetime: Singleton — stateless, safe to reuse across requests.
    /// </summary>
    public class PdfDocumentLoader : IDocumentLoader
    {
        // ── Fields ────────────────────────────────────────────────────────────

        private readonly ILogger<PdfDocumentLoader> _logger;

        private const string PdfExtension  = ".pdf";
        private const string PageSeparator = "\n\n";

        // ── Constructor ───────────────────────────────────────────────────────

        public PdfDocumentLoader(ILogger<PdfDocumentLoader> logger)
        {
            _logger = logger;
        }

        // ── IDocumentLoader Implementation ────────────────────────────────────

        /// <summary>
        /// Returns true if the file has a .pdf extension (case-insensitive).
        /// </summary>
        public bool CanLoad(string filePath)
        {
            return string.Equals(
                Path.GetExtension(filePath),
                PdfExtension,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Loads a single PDF file and returns a Document with extracted plain text.
        /// </summary>
        public async Task<Document> LoadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"PDF not found: {filePath}", filePath);

            if (!CanLoad(filePath))
                throw new NotSupportedException(
                    $"PdfDocumentLoader only handles .pdf files. Got: {Path.GetExtension(filePath)}");

            var fileName = Path.GetFileName(filePath);
            var title    = Path.GetFileNameWithoutExtension(filePath);

            _logger.LogInformation("Loading PDF: {FileName}", fileName);

            // iText7 is synchronous I/O — wrap in Task.Run to avoid blocking the thread pool.
            var content = await Task.Run(
                () => ExtractTextFromPdf(filePath, fileName),
                cancellationToken);

            var document = new Document
            {
                Id       = Guid.NewGuid().ToString(),
                Title    = title,
                FileName = fileName,
                Content  = content,
                Source   = filePath,
                LoadedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Loaded PDF '{FileName}': {Chars} chars.",
                fileName, document.ContentLength);

            if (!document.HasContent)
            {
                _logger.LogWarning(
                    "PDF '{FileName}' produced no text. " +
                    "It may be a scanned image-only PDF (OCR not supported).",
                    fileName);
            }

            return document;
        }

        /// <summary>
        /// Loads all PDF files in a folder and returns successfully loaded Documents.
        /// Files that fail to load are skipped with a warning.
        /// </summary>
        public async Task<IReadOnlyList<Document>> LoadFolderAsync(
            string folderPath,
            CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(folderPath))
            {
                _logger.LogWarning(
                    "Document folder not found: {Path}. Add PDF files here to enable RAG.",
                    folderPath);
                return Array.Empty<Document>();
            }

            var pdfFiles = Directory
                .GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly)
                .Where(CanLoad)
                .ToList();

            if (pdfFiles.Count == 0)
            {
                _logger.LogWarning("No PDF files found in '{Folder}'.", folderPath);
                return Array.Empty<Document>();
            }

            _logger.LogInformation(
                "Found {Count} PDF file(s) in '{Folder}'. Loading...",
                pdfFiles.Count, folderPath);

            var documents = new List<Document>();

            foreach (var filePath in pdfFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var doc = await LoadAsync(filePath, cancellationToken);
                    documents.Add(doc);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to load '{File}'. Skipping.", Path.GetFileName(filePath));
                }
            }

            _logger.LogInformation(
                "Loaded {Loaded}/{Total} PDF file(s) successfully.",
                documents.Count, pdfFiles.Count);

            return documents.AsReadOnly();
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Uses iText7 to extract plain text from every page of a PDF.
        ///
        /// iText7 extraction pipeline (per page):
        ///   1. PdfReader opens the file and decodes PDF encryption/compression
        ///   2. PdfTextExtractor calls the render listener on each glyph
        ///   3. LocationTextExtractionStrategy:
        ///      a. Groups glyphs into words by X-gap threshold
        ///      b. Sorts words by (Y descending, X ascending) = reading order
        ///      c. Returns reconstructed text string for the page
        ///   4. We append each page's text separated by a blank line
        /// </summary>
        private string ExtractTextFromPdf(string filePath, string fileName)
        {
            var contentBuilder = new StringBuilder();

            try
            {
                // PdfReader: reads and decodes the PDF binary format.
                // PdfDocument: the parsed document object (page tree, resources, etc.).
                using var reader  = new PdfReader(filePath);
                using var pdfDoc  = new PdfDocument(reader);

                int totalPages = pdfDoc.GetNumberOfPages();

                for (int pageNum = 1; pageNum <= totalPages; pageNum++)
                {
                    var page = pdfDoc.GetPage(pageNum);

                    // LocationTextExtractionStrategy:
                    //   Best general-purpose strategy in iText7.
                    //   Uses character bounding boxes to reconstruct reading order.
                    //   Handles multi-column layouts better than SimpleTextExtractionStrategy.
                    var strategy = new LocationTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);

                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        if (contentBuilder.Length > 0)
                            contentBuilder.Append(PageSeparator);

                        contentBuilder.Append(pageText.Trim());
                    }
                }

                _logger.LogDebug(
                    "Extracted text from {Pages} pages of '{File}'.",
                    totalPages, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "iText7 failed to parse '{File}'.", fileName);

                throw new InvalidOperationException(
                    $"Failed to extract text from '{fileName}': {ex.Message}", ex);
            }

            return contentBuilder.ToString();
        }
    }
}
