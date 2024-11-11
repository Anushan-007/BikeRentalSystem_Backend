namespace BikeRental_System3.DTOs.Response
{
    public class RentalRecordResponse
    {
        public Guid RecordId { get; set; }
        public DateTime? RentalOut { get; set; }
        public DateTime? RentalReturn { get; set; }
        public decimal? Payment { get; set; }
        public Guid RentalRequestId { get; set; }
        public string? RegistrationNumber { get; set; }
    }
}
