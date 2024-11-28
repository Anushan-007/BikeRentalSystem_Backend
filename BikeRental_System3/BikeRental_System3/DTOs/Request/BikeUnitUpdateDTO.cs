namespace BikeRental_System3.DTOs.Request
{
    public class BikeUnitUpdateDTO
    {
        public Guid UnitId { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string RegistrationNumber { get; set; }
        public int Year { get; set; }
        public int RentPerDay { get; set; }
        public bool Availability { get; set; } = true;

        public List<IFormFile> BikeImages { get; set; } = new List<IFormFile>();
    }
}
