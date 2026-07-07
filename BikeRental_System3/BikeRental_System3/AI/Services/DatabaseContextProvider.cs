using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IService;
using System.Text;

namespace BikeRental_System3.AI.Services
{
    /// <summary>
    /// Real-time context provider: queries live bike data from SQL Server
    /// and injects it into the {{context}} placeholder in the system prompt.
    ///
    /// WHY DATABASE INSTEAD OF PDF:
    ///   Static PDFs contain stale data. If a bike's price changes or a new bike
    ///   is added to the database, the PDF would be wrong. The database is always
    ///   the source of truth for structured, frequently-changing data.
    ///
    ///   Static PDFs are appropriate for: FAQs, Rental Terms, Policy documents
    ///   (things that rarely change and are authored as documents).
    ///
    ///   Database is appropriate for: bike inventory, pricing, availability
    ///   (things that change with business operations and are managed via API).
    ///
    /// WHAT THIS PRODUCES:
    ///   The formatted context string that fills {{context}} in bike-rental-system.txt:
    ///
    ///   === Heaven Bike Rental — Live Bike Inventory ===
    ///   Last updated: 2026-07-07 10:30 UTC
    ///
    ///   Bike: KTM Young RC
    ///     Type: Mountain
    ///     Rate: Rs. 170/hour
    ///     Available: 2 of 3 units
    ///     Available Units: KTM-001, KTM-002
    ///
    ///   Summary: 2 units available out of 3 total.
    ///
    /// DEPENDENCY INJECTION NOTE — IServiceScopeFactory:
    ///   IBikeService is registered as Scoped (new instance per HTTP request).
    ///   IContextProvider is registered as Singleton (one instance for app lifetime).
    ///   A Singleton cannot directly inject a Scoped service — this causes the
    ///   "captive dependency" problem (the Scoped service lives as long as the Singleton,
    ///   which is the entire app lifetime, defeating the purpose of Scoped lifetime).
    ///
    ///   Solution: IServiceScopeFactory
    ///   We inject the factory (which is Singleton-safe), then create a temporary
    ///   scope for each call to GetContextAsync. The Scoped services inside that
    ///   scope are properly created and disposed after the call completes.
    ///
    ///   This is the official .NET pattern for this scenario:
    ///   https://docs.microsoft.com/en-us/dotnet/core/extensions/scoped-service
    ///
    /// GRACEFUL FALLBACK:
    ///   If the database is unavailable, returns string.Empty.
    ///   The chat still works — GPT answers from its own training data.
    ///   The {{context}} placeholder in the prompt renders as blank.
    /// </summary>
    public class DatabaseContextProvider : IContextProvider
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        // IServiceScopeFactory is Singleton-safe.
        // We use it to create a temporary Scoped context for IBikeService access.
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseContextProvider> _logger;

        // ── Constructor ───────────────────────────────────────────────────────

        public DatabaseContextProvider(
            IServiceScopeFactory scopeFactory,
            ILogger<DatabaseContextProvider> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        // ── IContextProvider Implementation ───────────────────────────────────

        /// <summary>
        /// Fetches all bikes from the database and formats them as a context string.
        ///
        /// The userQuery parameter is received but not used for filtering in this
        /// implementation — all bikes are always included in context.
        ///
        /// Phase 4 enhancement: use userQuery to filter. For example:
        ///   "mountain bike available?" → only return mountain bike rows
        ///   "what is the price?" → include all pricing info
        /// This avoids sending large context for short conversations.
        /// </summary>
        public async Task<string> GetContextAsync(
            string userQuery,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(
                "DatabaseContextProvider: fetching bike inventory for query '{Query}'",
                userQuery.Length > 60 ? userQuery[..60] + "..." : userQuery);

            try
            {
                // ── Create a temporary Scoped service scope ────────────────────
                // This scope is disposed when the using block exits,
                // which also disposes IBikeService and its DbContext.
                using var scope = _scopeFactory.CreateScope();
                var bikeService = scope.ServiceProvider.GetRequiredService<IBikeService>();

                // ── Query the database ─────────────────────────────────────────
                // GetAllBikesAsync returns BikeResponse objects that include
                // a nested List<BikeUnitResponse> with Availability per unit.
                var bikes = await bikeService.GetAllBikesAsync();

                if (bikes == null || bikes.Count == 0)
                {
                    _logger.LogWarning(
                        "No bikes found in database. Context will be empty.");
                    return string.Empty;
                }

                _logger.LogDebug(
                    "Retrieved {Count} bike model(s) from database.",
                    bikes.Count);

                // ── Format context string ──────────────────────────────────────
                return FormatBikeInventoryContext(bikes);
            }
            catch (Exception ex)
            {
                // Graceful fallback: log the error but do not crash the chat.
                // The {{context}} placeholder will be empty and GPT will
                // answer from its own training knowledge.
                _logger.LogError(
                    ex,
                    "Failed to retrieve bike context from database. " +
                    "Chat will continue without inventory context.");

                return string.Empty;
            }
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Formats the list of bikes into a structured plain-text context block
        /// that GPT can read and use to answer inventory questions.
        ///
        /// Format is intentionally plain text (not JSON or HTML) because:
        ///   - GPT reads and reasons about plain text naturally
        ///   - Clear structure without markup noise
        ///   - Each bike entry is self-contained
        /// </summary>
        private string FormatBikeInventoryContext(List<BikeResponse> bikes)
        {
            var sb = new StringBuilder();

            // ── Header ─────────────────────────────────────────────────────────
            sb.AppendLine("=== Heaven Bike Rental — Live Bike Inventory ===");
            sb.AppendLine($"Data retrieved: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine();

            // ── Per-bike details ───────────────────────────────────────────────
            int index = 1;
            foreach (var bike in bikes)
            {
                var units         = bike.BikeUnits ?? new List<BikeUnitResponse>();
                var totalUnits    = units.Count;
                var availableUnits = units.Where(u => u.Availability).ToList();

                sb.AppendLine($"{index}. {bike.Brand} {bike.Model}");
                sb.AppendLine($"   Type          : {bike.Type}");
                sb.AppendLine($"   Rate          : Rs. {bike.RentPerHour}/hour");
                sb.AppendLine($"   Total units   : {totalUnits}");
                sb.AppendLine($"   Available now : {availableUnits.Count} unit(s)");

                if (availableUnits.Count > 0)
                {
                    // List available unit registration numbers and years
                    var unitDetails = availableUnits
                        .Select(u => $"{u.RegistrationNumber} ({u.Year})")
                        .ToList();

                    sb.AppendLine($"   Available IDs : {string.Join(", ", unitDetails)}");
                }
                else
                {
                    sb.AppendLine("   Available IDs : None — all units currently rented out");
                }

                sb.AppendLine();
                index++;
            }

            // ── Summary ────────────────────────────────────────────────────────
            int totalAvailable = bikes.Sum(b =>
                b.BikeUnits?.Count(u => u.Availability) ?? 0);
            int totalAll = bikes.Sum(b =>
                b.BikeUnits?.Count ?? 0);

            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine($"Fleet summary: {totalAvailable} of {totalAll} units currently available.");
            sb.AppendLine($"Bike models in catalog: {bikes.Count}");

            return sb.ToString();
        }
    }
}
