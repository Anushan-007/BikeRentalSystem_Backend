namespace BikeRental_System3.DTOs.Response
{
    public class BikeResponse
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public decimal RatePerHour { get; set; }
    }
}
