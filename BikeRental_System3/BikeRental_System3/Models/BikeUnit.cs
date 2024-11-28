using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.Models
{
    public class BikeUnit
    {
        [Key]
        public Guid UnitId { get; set; }
        public Guid BikeId { get; set; }
        public string RegistrationNumber { get; set; }
        public int Year { get; set; }

        public int RentPerDay { get; set; }
        public bool Availability { get; set; } = true;
        public bool IsDeleted { get; set; }
        public List<Image>? Images { get; set; }

        public Bike? Bike { get; set; }
        public ICollection<RentalRecord>? RentalRecords { get; set; }
        
    }
}
