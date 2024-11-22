namespace BikeRental_System3.DTOs.Request
{
    public class BikeUnitUpdateDTO
    {
        public Guid UnitId { get; set; }
        public string RegistrationNumber { get; set; }
        public int Year { get; set; }
        public int RentPerDay { get; set; }
        public List<IFormFile> BikeImages { get; set; } = new List<IFormFile>();
    }
}
