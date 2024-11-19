namespace BikeRental_System3.DTOs.Request
{
    public class BikeRequest
    {
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public decimal RatePerHour { get; set; }
        public IFormFile? Image { get; set; }
    }
}
