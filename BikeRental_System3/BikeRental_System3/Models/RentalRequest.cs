using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.Models
{
    public class RentalRequest
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime RequestTime { get; set; }
        public Status Status { get; set; }
        public Guid BikeId { get; set; }
        public bool? UserAlert { get; set; }
        public string NicNumber { get; set; }
        //public Guid BikeUnitId { get; set; }
        //public BikeUnit? BikeUnit {  get; set; }
        public Bike? Bike { get; set; }
        public User? User { get; set; }
        public RentalRecord? RentalRecord { get; set; }

    }
}
