using BikeRental_System3.AI.Models;

namespace BikeRental_System3.AI.Documents
{
    /// <summary>
    /// Contract for loading documents from the file system into the RAG pipeline.
    ///
    /// LangChain equivalent (Python):
    ///   from langchain.document_loaders import PyPDFLoader
    ///   loader = PyPDFLoader("bike-catalog.pdf")
    ///   docs = loader.load()
    ///
    /// This interface is the .NET equivalent — file-format agnostic.
    ///
    /// SOLID Principles applied:
    ///
    ///   S — Single Responsibility:
    ///     This interface only knows about loading files.
    ///     It has no knowledge of chunking, embedding, or retrieval.
    ///
    ///   O — Open/Closed Principle:
    ///     Add a new file format by adding a new class that implements this interface.
    ///     Existing implementations are never modified.
    ///
    ///   L — Liskov Substitution:
    ///     Any implementation can be used wherever IDocumentLoader is expected.
    ///
    ///   D — Dependency Inversion:
    ///     The chunker (Phase 4.2) will depend on IDocumentLoader, not PdfDocumentLoader.
    ///
    /// Implementations:
    ///   PdfDocumentLoader   → .pdf files (Phase 4.1)
    ///   WordDocumentLoader  → .docx files (future)
    ///   TextDocumentLoader  → .txt, .md files (future)
    ///
    /// CanLoad() pattern:
    ///   Enables a DocumentLoaderFactory to route files to the correct loader:
    ///   foreach (var loader in loaders)
    ///       if (loader.CanLoad(filePath)) return loader.LoadAsync(filePath);
    /// </summary>
    public interface IDocumentLoader
    {
        /// <summary>
        /// Returns true if this loader can handle the given file.
        ///
        /// Implementation checks the file extension:
        ///   PdfDocumentLoader  → ".pdf"
        ///   WordDocumentLoader → ".docx", ".doc"
        ///   TextDocumentLoader → ".txt", ".md"
        ///
        /// Used by DocumentLoaderFactory (Phase 4.2+) to route files automatically.
        /// Also used for validation before calling LoadAsync.
        /// </summary>
        bool CanLoad(string filePath);

        /// <summary>
        /// Loads a single file and returns a strongly typed Document.
        ///
        /// Steps performed by the implementation:
        ///   1. Validate the file exists and has the correct extension
        ///   2. Open the file (e.g., PdfDocument.Open(filePath))
        ///   3. Extract text from all pages
        ///   4. Assemble and return a Document object
        ///
        /// Throws:
        ///   FileNotFoundException    — file does not exist
        ///   InvalidOperationException — file is not a valid document (corrupted, encrypted)
        ///   NotSupportedException    — CanLoad(filePath) returned false
        ///
        /// CancellationToken: passed to any async I/O operations for graceful shutdown.
        /// </summary>
        Task<Document> LoadAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads all supported files from a folder.
        ///
        /// Implementation:
        ///   - Finds all files where CanLoad(filePath) == true
        ///   - Loads each one (in parallel or sequentially)
        ///   - Skips files that fail to load (logs the error, continues)
        ///   - Returns the successfully loaded documents
        ///
        /// Phase 4.1 use case:
        ///   Load the entire DocumentStore/ folder when the application starts.
        ///   Each document is then chunked and embedded (Phase 4.2+).
        ///
        /// Example:
        ///   folderPath = "C:\DocumentStore\"
        ///   returns [Document("bike-catalog.pdf"), Document("rental-policy.pdf")]
        /// </summary>
        Task<IReadOnlyList<Document>> LoadFolderAsync(
            string folderPath,
            CancellationToken cancellationToken = default);
    }
}
