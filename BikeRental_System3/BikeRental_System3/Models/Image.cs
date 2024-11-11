using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.Models
{
    public class Image
    {
        [Key]
        public Guid Id { get; set; }
        public string ImagePath { get; set; }
        public Guid BikeId { get; set; }

        public Bike? Bike { get; set; }
    }
}
