using BikeRental_System3.Models;

namespace BikeRental_System3.DTOs.Response
{
    public class UserResponse
    {
        public string NicNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        //public string Password { get; set; }
        public DateTime AccountCreated { get; set; }
        public Roles roles { get; set; }
        public bool? IsBlocked { get; set; } = false;
        public string UserName { get; set; }
        public string? ProfileImage { get; set; }
    }
}
