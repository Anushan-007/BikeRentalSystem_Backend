namespace BikeRental_System3.DTOs.Request
{
    public class ImageRequest
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; }
        public Guid BikeId { get; set; }
    }
}
