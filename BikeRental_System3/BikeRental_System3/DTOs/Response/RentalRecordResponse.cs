namespace BikeRental_System3.DTOs.Response
{
    public class RentalRecordResponse
    {
        public int RecordId { get; set; }
        public DateTime? RentalOut { get; set; }
        public DateTime? RentalReturn { get; set; }
        public decimal? Payment { get; set; }
        public int RentalRequestId { get; set; }
        public string? RegistrationNumber { get; set; }
    }
}
