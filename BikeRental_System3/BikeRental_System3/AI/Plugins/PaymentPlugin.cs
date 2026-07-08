using System.ComponentModel;
using System.Text;
using BikeRental_System3.IService;
using Microsoft.SemanticKernel;

namespace BikeRental_System3.AI.Plugins
{
    /// <summary>
    /// Semantic Kernel Plugin — Payment domain.
    ///
    /// Provides GPT with payment and billing information from SQL Server.
    /// GPT calls these functions when users ask about outstanding balances,
    /// payment history, or specific rental charges.
    ///
    /// Plugin calls: IRentalRecordService (existing service)
    /// Plugin NEVER accesses the database directly.
    ///
    /// SK naming: registered as "Payments" in BikeRentalChatChain.
    ///
    /// Note: This plugin reads payment data only.
    /// Actual payment processing is handled by staff via the admin UI.
    /// </summary>
    public sealed class PaymentPlugin
    {
        private readonly IRentalRecordService   _rentalRecordService;
        private readonly ILogger<PaymentPlugin> _logger;

        public PaymentPlugin(
            IRentalRecordService   rentalRecordService,
            ILogger<PaymentPlugin> logger)
        {
            _rentalRecordService = rentalRecordService ?? throw new ArgumentNullException(nameof(rentalRecordService));
            _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Functions ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns unpaid/outstanding rentals for a user.
        /// Triggered by: "Do I have outstanding payments?", "What do I owe?"
        /// </summary>
        [KernelFunction]
        [Description("Get outstanding (unpaid or overdue) rental payments for a user by their NIC number. Shows each overdue rental's ID, bike registration number, rental start time, and amount owed. Call this when a user asks about outstanding balance, what they owe, or unpaid rentals.")]
        public async Task<string> GetOutstandingPayments(
            [Description("The user's NIC number. Example: '199012345678'. Ask the user if you don't have this.")]
            string nicNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nicNumber))
                return "Please provide a NIC number to check outstanding payments.";

            _logger.LogInformation("PaymentPlugin.GetOutstandingPayments invoked for NIC: {Nic}", nicNumber);

            try
            {
                var overdueRentals = await _rentalRecordService.GetOverDueRentalsOfUser(nicNumber);

                if (overdueRentals == null || overdueRentals.Count == 0)
                    return $"No outstanding payments found for NIC: {nicNumber}. Your account is up to date.";

                var sb    = new StringBuilder($"Outstanding payments for {nicNumber}:\n\n");
                decimal total = 0;

                foreach (var r in overdueRentals)
                {
                    var amount = r.Payment ?? 0;
                    total += amount;

                    sb.AppendLine($"• Rental ID: {r.Id}");
                    sb.AppendLine($"  Registration No: {r.RegistrationNumber ?? "N/A"}");
                    sb.AppendLine($"  Rented Out: {r.RentalOut?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"}");
                    sb.AppendLine($"  Amount Due: Rs.{amount}");
                    sb.AppendLine();
                }

                sb.AppendLine($"Total Outstanding: Rs.{total}");

                _logger.LogInformation(
                    "PaymentPlugin.GetOutstandingPayments: {Count} overdue rental(s), total Rs.{Total} for {Nic}.",
                    overdueRentals.Count, total, nicNumber);

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PaymentPlugin.GetOutstandingPayments failed for NIC: {Nic}.", nicNumber);
                return "Unable to retrieve outstanding payment information at this time.";
            }
        }

        /// <summary>
        /// Returns payment details for a single rental record.
        /// Triggered by: "How much did rental XYZ cost?", "Payment for rental ID ..."
        /// </summary>
        [KernelFunction]
        [Description("Get the payment details (amount paid and hourly rate) for a specific rental record by its ID. Call this when a user asks about the cost or payment for a specific rental they've had.")]
        public async Task<string> GetPaymentDetails(
            [Description("The rental record ID (GUID). The user should have this from their rental history. Example: '3f2504e0-4f89-11d3-9a0c-0305e82c3301'")]
            string rentalRecordId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rentalRecordId))
                return "Please provide a rental record ID to get payment details.";

            if (!Guid.TryParse(rentalRecordId, out var recordGuid))
                return $"'{rentalRecordId}' is not a valid rental record ID format.";

            _logger.LogInformation("PaymentPlugin.GetPaymentDetails invoked for RecordId: {Id}", rentalRecordId);

            try
            {
                var payment = await _rentalRecordService.GetPayment(recordGuid);

                _logger.LogInformation(
                    "PaymentPlugin.GetPaymentDetails: Rs.{Amount} at Rs.{Rate}/hr for record {Id}.",
                    payment.Payment, payment.RatePerHour, rentalRecordId);

                return $"Payment details for rental {rentalRecordId}:\n\n" +
                       $"• Amount Charged: Rs.{payment.Payment}\n" +
                       $"• Hourly Rate:    Rs.{payment.RatePerHour}/hour";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PaymentPlugin.GetPaymentDetails failed for RecordId: {Id}.", rentalRecordId);
                return "Unable to retrieve payment details at this time. Please check the rental record ID.";
            }
        }

        /// <summary>
        /// Returns the total revenue collected (admin/manager utility).
        /// Triggered by: "What is the total revenue?", "How much has been collected?"
        /// </summary>
        [KernelFunction]
        [Description("Get the total revenue collected from all completed rentals across all customers. This is an administrative summary. Call this when staff asks about total earnings, total revenue, or overall payment collection.")]
        public async Task<string> GetTotalRevenue(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("PaymentPlugin.GetTotalRevenue invoked.");

            try
            {
                var total = await _rentalRecordService.GetTotalPaymentAsync();

                _logger.LogInformation("PaymentPlugin.GetTotalRevenue: Rs.{Total}.", total);

                return $"Total revenue collected from all rentals: Rs.{total}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PaymentPlugin.GetTotalRevenue failed.");
                return "Unable to retrieve total revenue at this time.";
            }
        }
    }
}
