using BikeRental_System3.Models;
using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.DTOs.Request
{
    public class RentalRequestRequest
    {
        [Required]
        public DateTime RequestTime { get; set; }
        [Required]
        public Guid BikeId { get; set; }
        [Required]
        public string NicNumber { get; set; }

        //public bool? UserAlert { get; set; }
        //public string NicNumber { get; set; }
        //public Status Status { get; set; }
    }
}
