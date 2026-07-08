using System.ComponentModel;
using System.Text;
using BikeRental_System3.IService;
using Microsoft.SemanticKernel;

namespace BikeRental_System3.AI.Plugins
{
    /// <summary>
    /// Semantic Kernel Plugin — Bike domain.
    ///
    /// Provides GPT with live bike inventory data from SQL Server.
    /// GPT automatically calls these functions when users ask about
    /// bike availability, types, pricing, or fleet details.
    ///
    /// Plugin calls: IBikeService, IBikeUnitService (existing services)
    /// Plugin NEVER accesses the database directly.
    ///
    /// SK naming: registered as "Bikes" in BikeRentalChatChain.
    /// Function names become: Bikes-GetAvailableBikes, Bikes-SearchBikes, etc.
    /// </summary>
    public sealed class BikePlugin
    {
        private readonly IBikeService     _bikeService;
        private readonly IBikeUnitService _bikeUnitService;
        private readonly ILogger<BikePlugin> _logger;

        public BikePlugin(
            IBikeService     bikeService,
            IBikeUnitService bikeUnitService,
            ILogger<BikePlugin> logger)
        {
            _bikeService     = bikeService     ?? throw new ArgumentNullException(nameof(bikeService));
            _bikeUnitService = bikeUnitService ?? throw new ArgumentNullException(nameof(bikeUnitService));
            _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Functions ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all bikes with brand, model, type, rate, and available unit count.
        /// Triggered by: "What bikes are available?", "Show me all bikes", "Bike list"
        /// </summary>
        [KernelFunction]
        [Description("Get all bikes in the fleet with their brand, model, type, rental rate per hour, and how many units are currently available. Call this when the user asks about available bikes, fleet, or wants to see all bike options.")]
        public async Task<string> GetAvailableBikes(
            CancellationToken cancellationToken = default)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("BikePlugin.GetAvailableBikes invoked.");

            try
            {
                var bikes = await _bikeService.GetAllBikesAsync();
                sw.Stop();

                if (bikes == null || bikes.Count == 0)
                    return "No bikes are currently registered in the system.";

                var sb = new StringBuilder($"Bike Fleet ({bikes.Count} model(s)):\n\n");

                foreach (var bike in bikes)
                {
                    var available = bike.BikeUnits?.Count(u => u.Availability) ?? 0;
                    var total     = bike.BikeUnits?.Count ?? 0;

                    sb.AppendLine($"• {bike.Brand} {bike.Model}");
                    sb.AppendLine($"  Type: {bike.Type}  |  Rate: Rs.{bike.RentPerHour}/hour");
                    sb.AppendLine($"  Available: {available}/{total} unit(s)");
                    sb.AppendLine();
                }

                _logger.LogInformation(
                    "BikePlugin.GetAvailableBikes returned {Count} bike(s) in {Ms} ms.",
                    bikes.Count, sw.ElapsedMilliseconds);

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BikePlugin.GetAvailableBikes failed.");
                return "Unable to retrieve bike information at this time. Please try again.";
            }
        }

        /// <summary>
        /// Searches bikes by brand, model, or type keyword.
        /// Triggered by: "Any Yamaha bikes?", "Show Sport bikes", "R15 available?"
        /// </summary>
        [KernelFunction]
        [Description("Search for bikes by brand name (e.g. 'Yamaha', 'Honda'), model name (e.g. 'R15', 'CB150'), or type (e.g. 'Sport', 'City', 'Cruiser'). Returns matching bikes with availability and pricing. Call this when the user asks about a specific brand, model, or bike category.")]
        public async Task<string> SearchBikes(
            [Description("The search keyword — brand name, model name, or bike type. Examples: 'Yamaha', 'R15', 'Sport', 'City'")]
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return "Please provide a brand, model, or type to search for.";

            _logger.LogInformation("BikePlugin.SearchBikes invoked. Term: '{Term}'", searchTerm);

            try
            {
                var bikes = await _bikeService.GetAllBikesAsync();
                var term  = searchTerm.Trim().ToLowerInvariant();

                var matched = bikes
                    .Where(b =>
                        b.Brand.ToLowerInvariant().Contains(term) ||
                        b.Model.ToLowerInvariant().Contains(term) ||
                        b.Type.ToLowerInvariant().Contains(term))
                    .ToList();

                if (matched.Count == 0)
                    return $"No bikes found matching '{searchTerm}'. Try searching by brand (Yamaha), model (R15), or type (Sport).";

                var sb = new StringBuilder($"Bikes matching '{searchTerm}' ({matched.Count} result(s)):\n\n");

                foreach (var bike in matched)
                {
                    var available = bike.BikeUnits?.Count(u => u.Availability) ?? 0;
                    sb.AppendLine($"• {bike.Brand} {bike.Model} ({bike.Type})");
                    sb.AppendLine($"  Rate: Rs.{bike.RentPerHour}/hour  |  Available: {available} unit(s)");
                    sb.AppendLine($"  Bike ID: {bike.Id}");
                    sb.AppendLine();
                }

                _logger.LogInformation(
                    "BikePlugin.SearchBikes returned {Count} result(s) for '{Term}'.",
                    matched.Count, searchTerm);

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BikePlugin.SearchBikes failed for term '{Term}'.", searchTerm);
                return "Unable to search bikes at this time. Please try again.";
            }
        }

        /// <summary>
        /// Returns total count of immediately rentable bike units.
        /// Triggered by: "How many bikes are available?", "Available count"
        /// </summary>
        [KernelFunction]
        [Description("Get the total number of bike units currently available for rental across all models. Call this when the user asks 'how many bikes are available' or wants a quick availability count.")]
        public async Task<string> GetAvailableCount(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("BikePlugin.GetAvailableCount invoked.");

            try
            {
                var available   = await _bikeUnitService.GetAvailableBikeUnitsCountAsync();
                var unavailable = await _bikeUnitService.GetUnavailableBikeUnitsCountAsync();
                var total       = available + unavailable;

                _logger.LogInformation(
                    "BikePlugin.GetAvailableCount: {Available}/{Total}.", available, total);

                return $"Fleet summary: {available} of {total} bike unit(s) are currently available for rental.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BikePlugin.GetAvailableCount failed.");
                return "Unable to retrieve availability count at this time.";
            }
        }

        /// <summary>
        /// Lists all distinct bike categories/types in the fleet.
        /// Triggered by: "What types of bikes do you have?", "Categories"
        /// </summary>
        [KernelFunction]
        [Description("Get all available bike types/categories in the rental fleet (e.g., Sport, City, Cruiser, Mountain). Call this when the user asks what types or categories of bikes are available.")]
        public async Task<string> GetBikeTypes(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("BikePlugin.GetBikeTypes invoked.");

            try
            {
                var types = await _bikeService.GetAllBikeTypesAsync();

                if (types == null || types.Count == 0)
                    return "No bike categories found in the system.";

                _logger.LogInformation(
                    "BikePlugin.GetBikeTypes returned {Count} type(s).", types.Count);

                return $"Available bike categories ({types.Count}): {string.Join(", ", types)}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BikePlugin.GetBikeTypes failed.");
                return "Unable to retrieve bike types at this time.";
            }
        }
    }
}
