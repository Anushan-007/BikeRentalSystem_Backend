namespace BikeRental_System3.DTOs.Request
{
    public class RentalRecordRequest
    {
        public DateTime RentalOut { get; set; }
        public DateTime? RentalReturn { get; set; }
        public decimal Payment { get; set; }
        public int RentalRequestId { get; set; }
        public string? RegistrationNumber { get; set; }
    }
}
