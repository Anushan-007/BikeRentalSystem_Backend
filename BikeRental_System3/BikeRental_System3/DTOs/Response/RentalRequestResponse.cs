using BikeRental_System3.Models;

namespace BikeRental_System3.DTOs.Response
{
    public class RentalRequestResponse
    {
        public Guid RentalRequestId { get; set; }
        public DateTime RequestTime { get; set; }
        public Status Status { get; set; }
        public Guid BikeId { get; set; }
        public bool? UserAlert { get; set; }
        public string NicNumber { get; set; }
    }
}
