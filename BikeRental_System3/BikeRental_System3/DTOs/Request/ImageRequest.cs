namespace BikeRental_System3.DTOs.Request
{
    public class ImageRequest
    {
        public Guid Id { get; set; }
        public IFormFile ImagePath { get; set; }
        public Guid UnitId { get; set; }
    }
}
