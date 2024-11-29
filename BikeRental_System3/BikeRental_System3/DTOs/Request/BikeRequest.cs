using BikeRental_System3.Models;

namespace BikeRental_System3.DTOs.Request
{
    public class BikeRequest
    {
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public int RentPerHour { get; set; }
        public List<BikeUnitRequest> BikeUnits { get; set; } = new List<BikeUnitRequest>();
        
    }
}


//public IFormFile? Image { get; set; }