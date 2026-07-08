using System.ComponentModel;
using BikeRental_System3.IService;
using Microsoft.SemanticKernel;

namespace BikeRental_System3.AI.Plugins
{
    /// <summary>
    /// Semantic Kernel Plugin — User / Member domain.
    ///
    /// Provides GPT with user profile and membership information from SQL Server.
    /// GPT calls these functions when users ask about account details, membership
    /// status, or profile information.
    ///
    /// Plugin calls: IUserService (existing service)
    /// Plugin NEVER accesses the database directly.
    ///
    /// SK naming: registered as "Users" in BikeRentalChatChain.
    ///
    /// Privacy note: Only NIC-holder or staff should query profiles.
    /// GPT must ask the user for their NIC before calling GetUserProfile.
    /// </summary>
    public sealed class UserPlugin
    {
        private readonly IUserService     _userService;
        private readonly ILogger<UserPlugin> _logger;

        public UserPlugin(
            IUserService     userService,
            ILogger<UserPlugin> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Functions ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the full profile of a user identified by NIC.
        /// Triggered by: "What is my profile?", "Show my account", "My membership"
        /// </summary>
        [KernelFunction]
        [Description("Get the full profile details of a customer by their NIC number. Returns name, email, contact number, address, membership role (User/Manager/Admin), account status (Active/Blocked), and registration date. Call this when a user asks about their account details, profile, or membership information.")]
        public async Task<string> GetUserProfile(
            [Description("The user's NIC number (National Identity Card). Example: '199012345678' or '990123456V'. Must be provided by the user — never guess it.")]
            string nicNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nicNumber))
                return "Please provide your NIC number to look up your profile.";

            _logger.LogInformation("UserPlugin.GetUserProfile invoked for NIC: {Nic}", nicNumber);

            try
            {
                var user = await _userService.GetUserById(nicNumber);

                if (user == null)
                    return $"No account found with NIC: {nicNumber}. Please check the NIC number or register a new account.";

                var status = user.IsBlocked == true ? "⚠ Blocked" : "Active";

                _logger.LogInformation("UserPlugin.GetUserProfile returned data for NIC: {Nic}.", nicNumber);

                return $"Account Profile:\n\n" +
                       $"• Name:       {user.FirstName} {user.LastName}\n" +
                       $"• NIC:        {user.NicNumber}\n" +
                       $"• Username:   {user.UserName}\n" +
                       $"• Email:      {user.Email}\n" +
                       $"• Contact:    {user.ContactNo}\n" +
                       $"• Address:    {user.Address}\n" +
                       $"• Role:       {user.roles}\n" +
                       $"• Status:     {status}\n" +
                       $"• Member Since: {user.AccountCreated:yyyy-MM-dd}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserPlugin.GetUserProfile failed for NIC: {Nic}.", nicNumber);
                return "Unable to retrieve user profile at this time.";
            }
        }

        /// <summary>
        /// Returns only the membership tier and account status of a user.
        /// Triggered by: "What is my membership?", "Am I a premium member?"
        /// </summary>
        [KernelFunction]
        [Description("Get the membership role and account status for a user by their NIC number. Roles: User (standard member), Manager, Admin. Call this when a user specifically asks about their membership level, account role, or whether their account is active.")]
        public async Task<string> GetMembership(
            [Description("The user's NIC number. Example: '199012345678'. Must be provided by the user.")]
            string nicNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nicNumber))
                return "Please provide your NIC number to check membership status.";

            _logger.LogInformation("UserPlugin.GetMembership invoked for NIC: {Nic}", nicNumber);

            try
            {
                var user = await _userService.GetUserById(nicNumber);

                if (user == null)
                    return $"No account found with NIC: {nicNumber}.";

                var blocked = user.IsBlocked == true;

                _logger.LogInformation(
                    "UserPlugin.GetMembership: NIC {Nic} has role {Role}, blocked: {Blocked}.",
                    nicNumber, user.roles, blocked);

                return blocked
                    ? $"{user.FirstName} {user.LastName} — your account is currently blocked. " +
                      "Please contact support to resolve this."
                    : $"{user.FirstName} {user.LastName} — {user.roles} member. Account is active.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserPlugin.GetMembership failed for NIC: {Nic}.", nicNumber);
                return "Unable to retrieve membership information at this time.";
            }
        }
    }
}
