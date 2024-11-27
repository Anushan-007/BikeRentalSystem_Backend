using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.Models
{
    public class RentalRecord
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime? RentalOut { get; set; }
        public DateTime? RentalReturn { get; set; }
        public decimal? Payment {  get; set; }
        public string? RegistrationNumber { get; set; }

        //public string Feedback { get; set; }
        public Guid RentalRequestId { get; set; }
        public Guid? UnitId { get; set; }
        public RentalRequest? RentalRequest { get; set; }
        public BikeUnit? bikeUnits { get; set; }
    }
}
