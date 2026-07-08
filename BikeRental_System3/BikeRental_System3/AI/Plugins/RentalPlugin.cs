using System.ComponentModel;
using System.Text;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using Microsoft.SemanticKernel;

namespace BikeRental_System3.AI.Plugins
{
    /// <summary>
    /// Semantic Kernel Plugin — Rental Records domain.
    ///
    /// Provides GPT with live rental data: active rentals, rental history,
    /// overdue returns. Works on RentalRecord entities (actual bike-out events).
    ///
    /// Plugin calls: IRentalRecordService, IRentalRequestService (existing services)
    /// Plugin NEVER accesses the database directly.
    ///
    /// Distinguish from BookingPlugin:
    ///   RentalPlugin  → RentalRecord  (bike has physically left the shop)
    ///   BookingPlugin → RentalRequest (customer's booking/reservation request)
    ///
    /// SK naming: registered as "Rentals" in BikeRentalChatChain.
    /// </summary>
    public sealed class RentalPlugin
    {
        private readonly IRentalRecordService  _rentalRecordService;
        private readonly IRentalRequestService _rentalRequestService;
        private readonly ILogger<RentalPlugin> _logger;

        public RentalPlugin(
            IRentalRecordService  rentalRecordService,
            IRentalRequestService rentalRequestService,
            ILogger<RentalPlugin> logger)
        {
            _rentalRecordService  = rentalRecordService  ?? throw new ArgumentNullException(nameof(rentalRecordService));
            _rentalRequestService = rentalRequestService ?? throw new ArgumentNullException(nameof(rentalRequestService));
            _logger               = logger               ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Functions ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns active/ongoing rentals for a specific user.
        /// Triggered by: "What is my current rental?", "Am I currently renting anything?"
        /// </summary>
        [KernelFunction]
        [Description("Get the current active (ongoing) rentals for a user identified by their NIC number. A rental is active when the bike has been taken out but not yet returned. Call this when a user asks about their current rental, active rental, or what bike they currently have.")]
        public async Task<string> GetActiveRentals(
            [Description("The user's NIC number (National Identity Card). Example: '199012345678' or '990123456V'. Ask the user if you don't have this.")]
            string nicNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nicNumber))
                return "Please provide a NIC number to look up active rentals.";

            _logger.LogInformation("RentalPlugin.GetActiveRentals invoked for NIC: {Nic}", nicNumber);

            try
            {
                var sw      = System.Diagnostics.Stopwatch.StartNew();
                var records = await _rentalRecordService.GetRentalRecords(State.Incompleted);
                sw.Stop();

                var userRecords = records
                    .Where(r => string.Equals(
                        r.RentalRequest?.NicNumber, nicNumber,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (userRecords.Count == 0)
                    return $"No active rentals found for NIC: {nicNumber}. The user currently has no bike checked out.";

                var sb = new StringBuilder($"Active rentals for {nicNumber} ({userRecords.Count} rental(s)):\n\n");

                foreach (var r in userRecords)
                {
                    var bike = r.RentalRequest?.Bike;
                    sb.AppendLine($"• Rental ID: {r.Id}");
                    sb.AppendLine($"  Bike: {bike?.Brand} {bike?.Model} ({bike?.Type})");
                    sb.AppendLine($"  Registration No: {r.RegistrationNumber ?? "N/A"}");
                    sb.AppendLine($"  Rented Out: {r.RentalOut?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"}");
                    sb.AppendLine($"  Rate: Rs.{bike?.RentPerHour}/hour");
                    sb.AppendLine();
                }

                _logger.LogInformation(
                    "RentalPlugin.GetActiveRentals returned {Count} record(s) for {Nic} in {Ms} ms.",
                    userRecords.Count, nicNumber, sw.ElapsedMilliseconds);

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RentalPlugin.GetActiveRentals failed for NIC: {Nic}.", nicNumber);
                return "Unable to retrieve active rental information at this time.";
            }
        }

        /// <summary>
        /// Returns all rental requests (history) for a user.
        /// Triggered by: "Show my rental history", "Past rentals", "Previous bookings"
        /// </summary>
        [KernelFunction]
        [Description("Get the complete rental history for a user by their NIC number. Includes all past booking requests with their statuses (Pending, Accepted, Declined, OnRent). Call this when a user asks about their rental history, past rentals, or booking records.")]
        public async Task<string> GetRentalHistory(
            [Description("The user's NIC number. Example: '199012345678'. Ask the user if you don't have this.")]
            string nicNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nicNumber))
                return "Please provide a NIC number to retrieve rental history.";

            _logger.LogInformation("RentalPlugin.GetRentalHistory invoked for NIC: {Nic}", nicNumber);

            try
            {
                var requests = await _rentalRequestService.GetRentalRequestbyNic(nicNumber);

                if (requests == null || requests.Count == 0)
                    return $"No rental history found for NIC: {nicNumber}.";

                var sb = new StringBuilder($"Rental history for {nicNumber} ({requests.Count} request(s)):\n\n");

                foreach (var r in requests.OrderByDescending(x => x.RequestTime))
                {
                    sb.AppendLine($"• Request ID: {r.Id}");
                    sb.AppendLine($"  Bike: {r.Bike?.Brand} {r.Bike?.Model} ({r.Bike?.Type})");
                    sb.AppendLine($"  Status: {r.Status}");
                    sb.AppendLine($"  Requested: {r.RequestTime:yyyy-MM-dd HH:mm}");
                    sb.AppendLine();
                }

                _logger.LogInformation(
                    "RentalPlugin.GetRentalHistory returned {Count} request(s) for {Nic}.",
                    requests.Count, nicNumber);

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RentalPlugin.GetRentalHistory failed for NIC: {Nic}.", nicNumber);
                return "Unable to retrieve rental history at this time.";
            }
        }

        /// <summary>
        /// Returns overdue (unreturned, late) rentals for a user.
        /// Triggered by: "Do I have overdue rentals?", "Late return"
        /// </summary>
        [KernelFunction]
        [Description("Get overdue or late rentals for a user by their NIC number. These are rentals where the bike has not been returned on time. Call this when a user asks about overdue rentals, late returns, or penalties.")]
        public async Task<string> GetOverdueRentals(
            [Description("The user's NIC number. Example: '199012345678'. Ask the user if you don't have this.")]
            string nicNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nicNumber))
                return "Please provide a NIC number to check for overdue rentals.";

            _logger.LogInformation("RentalPlugin.GetOverdueRentals invoked for NIC: {Nic}", nicNumber);

            try
            {
                var overdue = await _rentalRecordService.GetOverDueRentalsOfUser(nicNumber);

                if (overdue == null || overdue.Count == 0)
                    return $"No overdue rentals found for NIC: {nicNumber}. All rentals are on time.";

                var sb = new StringBuilder($"Overdue rentals for {nicNumber} ({overdue.Count} overdue rental(s)):\n\n");

                foreach (var r in overdue)
                {
                    sb.AppendLine($"• Rental ID: {r.Id}");
                    sb.AppendLine($"  Registration No: {r.RegistrationNumber ?? "N/A"}");
                    sb.AppendLine($"  Rented Out: {r.RentalOut?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"}");
                    if (r.Payment.HasValue)
                        sb.AppendLine($"  Accrued Amount: Rs.{r.Payment.Value}");
                    sb.AppendLine();
                }

                _logger.LogInformation(
                    "RentalPlugin.GetOverdueRentals returned {Count} overdue record(s) for {Nic}.",
                    overdue.Count, nicNumber);

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RentalPlugin.GetOverdueRentals failed for NIC: {Nic}.", nicNumber);
                return "Unable to retrieve overdue rental information at this time.";
            }
        }

        /// <summary>
        /// Returns the status of a specific rental request.
        /// Triggered by: "What happened to my booking request?"
        /// </summary>
        [KernelFunction]
        [Description("Get the current status of a specific rental request by its ID. Statuses: Pending (waiting for approval), Accepted, Declined, OnRent (currently being rented). Call this when a user wants to know the status of a specific booking request.")]
        public async Task<string> GetRentalStatus(
            [Description("The rental request ID (GUID). The user should have received this when creating a booking. Example: '3f2504e0-4f89-11d3-9a0c-0305e82c3301'")]
            string rentalRequestId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rentalRequestId))
                return "Please provide the rental request ID to check its status.";

            if (!Guid.TryParse(rentalRequestId, out var requestGuid))
                return "The provided rental request ID is not in a valid format. Please provide a valid GUID.";

            _logger.LogInformation("RentalPlugin.GetRentalStatus invoked for RequestId: {Id}", rentalRequestId);

            try
            {
                var request = await _rentalRequestService.GetRentalRequest(requestGuid);

                if (request == null)
                    return $"No rental request found with ID: {rentalRequestId}.";

                _logger.LogInformation(
                    "RentalPlugin.GetRentalStatus returned status '{Status}' for request {Id}.",
                    request.Status, rentalRequestId);

                return $"Rental request status:\n" +
                       $"  Request ID: {request.Id}\n" +
                       $"  Bike: {request.Bike?.Brand} {request.Bike?.Model}\n" +
                       $"  Status: {request.Status}\n" +
                       $"  Requested: {request.RequestTime:yyyy-MM-dd HH:mm}\n" +
                       $"  Customer NIC: {request.NicNumber}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RentalPlugin.GetRentalStatus failed for RequestId: {Id}.", rentalRequestId);
                return "Unable to retrieve rental status at this time.";
            }
        }
    }
}
