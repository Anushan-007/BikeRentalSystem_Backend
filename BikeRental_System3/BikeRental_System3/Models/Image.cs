using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BikeRental_System3.Models
{
    public class Image
    {
     
        [Key]
        public Guid Id { get; set; }

        public Guid UnitId { get; set; }

        public string ImagePath { get; set; }

        public BikeUnit? BikeUnit { get; set; }
    }
}
