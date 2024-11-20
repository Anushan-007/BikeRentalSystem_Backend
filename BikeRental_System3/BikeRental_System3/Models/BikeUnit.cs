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
        public List<Image>? Images { get; set; }

        public Bike? Bike { get; set; }
    }
}
