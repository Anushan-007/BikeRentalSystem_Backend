using BikeRental_System3.Models;
using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.DTOs.Request
{
    public class AdminCreateRequest
    {
        [Required(ErrorMessage = "NIC Number is required")]
        [StringLength(12, ErrorMessage = "NIC Number should not exceed 12 characters")]
        public string NicNumber { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name should not exceed 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name should not exceed 50 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [Phone(ErrorMessage = "Invalid contact number format")]
        public string ContactNo { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address should not exceed 200 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username should be between 3 and 30 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password should be at least 6 characters long")]
        public string Password { get; set; }
    }
}