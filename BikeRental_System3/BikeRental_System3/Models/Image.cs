using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int BikeId { get; set; }

        public Bike? Bike { get; set; }
    }
}
