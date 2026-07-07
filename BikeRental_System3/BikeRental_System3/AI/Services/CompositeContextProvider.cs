using BikeRental_System3.AI.Interfaces;

namespace BikeRental_System3.AI.Services
{
    /// <summary>
    /// Combines DatabaseContextProvider (live bike inventory) and
    /// PdfContextProvider (FAQ + Terms PDFs) into a single context string
    /// that is injected into the GPT system prompt.
    ///
    /// WHY A COMPOSITE:
    ///   GPT needs two kinds of knowledge to answer customer questions:
    ///
    ///   1. Live data  → "Which bikes are available right now? What is the price?"
    ///      Source: SQL Server (via DatabaseContextProvider)
    ///      Changes: every rental/return
    ///
    ///   2. Static policy → "Is advance payment required? What are the rental terms?"
    ///      Source: FAQ.pdf, Terms.pdf (via PdfContextProvider)
    ///      Changes: rarely (requires server restart to pick up new PDFs)
    ///
    ///   The composite fetches both IN PARALLEL (Task.WhenAll) to minimise latency,
    ///   then concatenates the results with a blank line separator.
    ///
    /// RESULT INJECTED INTO PROMPT ({{context}} placeholder):
    ///
    ///   === Heaven Bike Rental — Live Bike Inventory ===
    ///   Data retrieved: 2026-07-07 10:30 UTC
    ///
    ///   1. Ktm Rc
    ///      Rate: Rs. 170/hour
    ///      Available IDs: R001 (2020)
    ///   ...
    ///
    ///   === Heaven Bike Rental — Policy & FAQ Documents ===
    ///
    ///   --- FAQ ---
    ///   Q10: How do I book a bike in advance?
    ///   A: A 30% advance payment is required to confirm a booking.
    ///   ...
    ///
    /// REGISTERED AS: IContextProvider (replaces the old DatabaseContextProvider registration)
    /// DatabaseContextProvider and PdfContextProvider are registered directly (not as IContextProvider)
    /// so CompositeContextProvider can inject them by concrete type.
    ///
    /// Lifetime: Singleton — all dependencies are Singleton-safe.
    /// </summary>
    public class CompositeContextProvider : IContextProvider
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        // Injected as concrete types so they are not confused with IContextProvider
        // (which this class itself implements — registering by interface would cause
        //  circular resolution if DI tried to inject IContextProvider here).
        private readonly DatabaseContextProvider _dbProvider;
        private readonly PdfContextProvider      _pdfProvider;
        private readonly ILogger<CompositeContextProvider> _logger;

        // ── Constructor ───────────────────────────────────────────────────────

        public CompositeContextProvider(
            DatabaseContextProvider dbProvider,
            PdfContextProvider pdfProvider,
            ILogger<CompositeContextProvider> logger)
        {
            _dbProvider  = dbProvider;
            _pdfProvider = pdfProvider;
            _logger      = logger;
        }

        // ── IContextProvider Implementation ───────────────────────────────────

        /// <summary>
        /// Fetches DB inventory and PDF documents IN PARALLEL, then concatenates.
        ///
        /// If either source fails, it returns string.Empty for that source
        /// (both providers have their own graceful fallback — see their catch blocks).
        /// The other source's context is still used, so partial failures are handled.
        ///
        /// The userQuery is passed through to each provider.
        /// DatabaseContextProvider currently ignores it (returns all bikes).
        /// PdfContextProvider ignores it (returns all cached PDF text).
        /// Phase 4 enhancement: use userQuery to filter/retrieve only relevant chunks.
        /// </summary>
        public async Task<string> GetContextAsync(
            string userQuery,
            CancellationToken cancellationToken = default)
        {
            // Run both providers in parallel — DB query and PDF cache read happen
            // simultaneously so total latency = max(dbTime, pdfTime), not db+pdf.
            var dbTask  = _dbProvider.GetContextAsync(userQuery, cancellationToken);
            var pdfTask = _pdfProvider.GetContextAsync(userQuery, cancellationToken);

            await Task.WhenAll(dbTask, pdfTask);

            var dbContext  = dbTask.Result;
            var pdfContext = pdfTask.Result;

            // Combine non-empty parts with a blank line separator
            var parts = new[] { dbContext, pdfContext }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            if (parts.Length == 0)
            {
                _logger.LogWarning(
                    "CompositeContextProvider: both DB and PDF returned empty context. " +
                    "GPT will answer from general training knowledge.");
                return string.Empty;
            }

            _logger.LogDebug(
                "CompositeContextProvider: combined {Count} context source(s).", parts.Length);

            return string.Join("\n\n", parts);
        }
    }
}
