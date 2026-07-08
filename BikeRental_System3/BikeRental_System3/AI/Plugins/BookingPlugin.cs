using System.ComponentModel;
using System.Text;
using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using Microsoft.SemanticKernel;

namespace BikeRental_System3.AI.Plugins
{
    /// <summary>
    /// Semantic Kernel Plugin — Booking / Rental Request domain.
    ///
    /// Provides GPT with the ability to read and create rental booking requests.
    /// A RentalRequest is the customer's reservation — it exists BEFORE the bike
    /// physically leaves the shop (which creates a RentalRecord, handled by RentalPlugin).
    ///
    /// Plugin calls: IRentalRequestService, IBikeService (existing services)
    /// Plugin NEVER accesses the database directly.
    ///
    /// SK naming: registered as "Bookings" in BikeRentalChatChain.
    ///
    /// Write operations (CreateBooking) include validation before submission.
    /// </summary>
    public sealed class BookingPlugin
    {
        private readonly IRentalRequestService _rentalRequestService;
        private readonly IBikeService          _bikeService;
        private readonly ILogger<BookingPlugin> _logger;

        public BookingPlugin(
            IRentalRequestService  rentalRequestService,
            IBikeService           bikeService,
            ILogger<BookingPlugin> logger)
        {
            _rentalRequestService = rentalRequestService ?? throw new ArgumentNullException(nameof(rentalRequestService));
            _bikeService          = bikeService          ?? throw new ArgumentNullException(nameof(bikeService));
            _logger               = logger               ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Functions ─────────────────────────────────────────────────────────

        /// <summary>
        /// Gets all booking requests made by a user.
        /// Triggered by: "Show my bookings", "What are my pending requests?"
        /// </summary>
        [KernelFunction]
        [Description("Get all booking requests (reservation history) for a user by their NIC number. Shows each request's bike details, status (Pending/Accepted/Declined/OnRent), and request date. Call this when a user asks to see their bookings, reservation history, or pending requests.")]
        public async Task<string> GetUserBookings(
            [Description("The user's NIC number. Example: '199012345678'. Ask the user if you don't have this.")]
            string nicNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nicNumber))
                return "Please provide a NIC number to view bookings.";

            _logger.LogInformation("BookingPlugin.GetUserBookings invoked for NIC: {Nic}", nicNumber);

            try
            {
                var requests = await _rentalRequestService.GetRentalRequestbyNic(nicNumber);

                if (requests == null || requests.Count == 0)
                    return $"No booking requests found for NIC: {nicNumber}.";

                var sb = new StringBuilder($"Booking requests for {nicNumber} ({requests.Count} request(s)):\n\n");

                foreach (var r in requests.OrderByDescending(x => x.RequestTime))
                {
                    sb.AppendLine($"• Booking ID: {r.Id}");
                    sb.AppendLine($"  Bike: {r.Bike?.Brand} {r.Bike?.Model} ({r.Bike?.Type})");
                    sb.AppendLine($"  Rate: Rs.{r.Bike?.RentPerHour}/hour");
                    sb.AppendLine($"  Status: {r.Status}");
                    sb.AppendLine($"  Requested On: {r.RequestTime:yyyy-MM-dd HH:mm}");
                    sb.AppendLine();
                }

                _logger.LogInformation(
                    "BookingPlugin.GetUserBookings returned {Count} request(s) for {Nic}.",
                    requests.Count, nicNumber);

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BookingPlugin.GetUserBookings failed for NIC: {Nic}.", nicNumber);
                return "Unable to retrieve booking information at this time.";
            }
        }

        /// <summary>
        /// Creates a new rental booking request.
        /// Triggered by: "Book Yamaha R15 for tomorrow", "Reserve a Sport bike"
        /// IMPORTANT: This is a write operation — only call after GPT confirms with user.
        /// </summary>
        [KernelFunction]
        [Description("Create a new bike rental booking request. Requires the user's NIC number, the bike ID they want to book (get from GetAvailableBikes or SearchBikes), and the requested rental date/time. The booking will be Pending until staff approves it. Always confirm the details with the user before calling this function.")]
        public async Task<string> CreateBooking(
            [Description("The user's NIC number. Example: '199012345678'")]
            string nicNumber,
            [Description("The bike ID (GUID) to book. Get this from GetAvailableBikes or SearchBikes. Example: '3f2504e0-4f89-11d3-9a0c-0305e82c3301'")]
            string bikeId,
            [Description("The requested rental date and time in ISO 8601 format. Example: '2026-07-15T09:00:00'. Must be a future date.")]
            string requestedDateTime,
            CancellationToken cancellationToken = default)
        {
            // ── Validate inputs ───────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(nicNumber))
                return "Cannot create booking: NIC number is required.";

            if (!Guid.TryParse(bikeId, out var bikeGuid))
                return $"Cannot create booking: '{bikeId}' is not a valid bike ID. Please get the bike ID from the available bikes list.";

            if (!DateTime.TryParse(requestedDateTime, out var requestTime))
                return $"Cannot create booking: '{requestedDateTime}' is not a valid date/time. Use format: 2026-07-15T09:00:00";

            if (requestTime < DateTime.UtcNow.AddMinutes(-5)) // small buffer for clock skew
                return "Cannot create booking: The requested date/time must be in the future.";

            _logger.LogInformation(
                "BookingPlugin.CreateBooking invoked. NIC: {Nic}, BikeId: {BikeId}, Time: {Time}",
                nicNumber, bikeId, requestTime);

            try
            {
                var request = new RentalRequestRequest
                {
                    NicNumber   = nicNumber,
                    BikeId      = bikeGuid,
                    RequestTime = requestTime
                };

                var result = await _rentalRequestService.PostRentalRequest(request);

                _logger.LogInformation(
                    "BookingPlugin.CreateBooking created request {Id} for NIC: {Nic}.",
                    result.Id, nicNumber);

                return $"Booking request created successfully!\n\n" +
                       $"• Booking ID: {result.Id}\n" +
                       $"• Bike: {result.Bike?.Brand} {result.Bike?.Model}\n" +
                       $"• Requested for: {requestTime:yyyy-MM-dd HH:mm}\n" +
                       $"• Status: {result.Status}\n\n" +
                       $"Your request is pending staff approval. You will be notified once it is reviewed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "BookingPlugin.CreateBooking failed. NIC: {Nic}, BikeId: {BikeId}.", nicNumber, bikeId);
                return "Unable to create booking at this time. Please try again or contact staff directly.";
            }
        }

        /// <summary>
        /// Cancels / deletes a pending booking request.
        /// Triggered by: "Cancel my booking", "Cancel request ID ..."
        /// </summary>
        [KernelFunction]
        [Description("Cancel a pending booking request by its ID. Only Pending requests can be cancelled. Accepted or OnRent requests cannot be cancelled through this function. Confirm the cancellation with the user before calling this.")]
        public async Task<string> CancelBooking(
            [Description("The booking request ID (GUID) to cancel. The user received this when creating the booking. Example: '3f2504e0-4f89-11d3-9a0c-0305e82c3301'")]
            string bookingId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(bookingId))
                return "Please provide the booking ID to cancel.";

            if (!Guid.TryParse(bookingId, out var bookingGuid))
                return $"'{bookingId}' is not a valid booking ID format.";

            _logger.LogInformation("BookingPlugin.CancelBooking invoked for BookingId: {Id}", bookingId);

            try
            {
                var result = await _rentalRequestService.DeleteRentalRequest(bookingGuid);

                _logger.LogInformation("BookingPlugin.CancelBooking succeeded for BookingId: {Id}.", bookingId);

                return $"Booking {bookingId} has been successfully cancelled. {result}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BookingPlugin.CancelBooking failed for BookingId: {Id}.", bookingId);
                return "Unable to cancel the booking at this time. The booking may already be accepted or not exist.";
            }
        }

        /// <summary>
        /// Gets total number of pending booking requests (admin utility).
        /// Triggered by: "How many pending bookings are there?"
        /// </summary>
        [KernelFunction]
        [Description("Get the total count of pending (waiting for approval) booking requests across all users. Useful for staff to understand workload. Call this when asked about the number of pending requests or approval queue.")]
        public async Task<string> GetPendingBookingsCount(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("BookingPlugin.GetPendingBookingsCount invoked.");

            try
            {
                var count = await _rentalRequestService.GetPendingRentalRequestsCountAsync();

                _logger.LogInformation("BookingPlugin.GetPendingBookingsCount: {Count} pending.", count);

                return $"There are currently {count} pending booking request(s) awaiting staff approval.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BookingPlugin.GetPendingBookingsCount failed.");
                return "Unable to retrieve pending bookings count at this time.";
            }
        }
    }
}
