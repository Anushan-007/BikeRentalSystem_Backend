using BikeRental_System3.Models;

namespace BikeRental_System3.DTOs.Request
{
    public class BikeRequest
    {
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public List<BikeUnit> BikeUnits { get; set; } = new List<BikeUnit>();
        //public IFormFile? Image { get; set; }
    }
}
