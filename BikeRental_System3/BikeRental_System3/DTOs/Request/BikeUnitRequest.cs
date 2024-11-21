namespace BikeRental_System3.DTOs.Request
{
    public class BikeUnitRequest
    {
        public Guid BikeId { get; set; }
        public string RegistrationNumber { get; set; }
        public int Year { get; set; }
        public int RentPerDay { get; set; }
        
        //public List<IFormFile>? Images { get; set; }  // Single image upload

    }
}
