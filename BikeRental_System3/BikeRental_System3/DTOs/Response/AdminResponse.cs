using BikeRental_System3.Models;

namespace BikeRental_System3.DTOs.Response
{
    public class AdminDashboardResponse
    {
        public int TotalUsers { get; set; }
        public int BlockedUsers { get; set; }
        public int AdminUsers { get; set; }
        public int ManagerUsers { get; set; }
        public int RegularUsers { get; set; }
        public int ActiveRentals { get; set; }
        public int PendingRequests { get; set; }
        public List<UserResponse> RecentUsers { get; set; } = new List<UserResponse>();
        public List<RentalRequestResponse> RecentRentals { get; set; } = new List<RentalRequestResponse>();
    }

    public class AdminCreateResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public UserResponse CreatedUser { get; set; }
    }

    public class RoleChangeResponse
    {
        public string Message { get; set; }
        public string NicNumber { get; set; }
        public Roles OldRole { get; set; }
        public Roles NewRole { get; set; }
        public DateTime ChangedAt { get; set; }
        public UserResponse UpdatedUser { get; set; }
    }
}