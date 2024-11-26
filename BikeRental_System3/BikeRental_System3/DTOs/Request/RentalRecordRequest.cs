namespace BikeRental_System3.DTOs.Request
{
    public class RentalRecordRequest
    {
        public DateTime RentalOut { get; set; }
        public DateTime? RentalReturn { get; set; }
        public decimal Payment { get; set; }
        public Guid RentalRequestId { get; set; }
        public string? BikeRegNo { get; set; }
    }
}
