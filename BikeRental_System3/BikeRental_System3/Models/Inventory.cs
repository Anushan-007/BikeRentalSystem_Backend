using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.Models
{
    public class Inventory
    {
        [Key]
        public string RegistrationNumber { get; set; }
        public int YearofManufacture { get; set; }
        public bool Availability { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsDeleted { get; set; }
        public int BikeId { get; set; }
        public Bike? Bike { get; set; }
        public ICollection<RentalRecord>? RentalRecords { get; set; }

    }
}
