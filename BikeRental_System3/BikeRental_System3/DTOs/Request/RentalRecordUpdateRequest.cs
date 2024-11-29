namespace BikeRental_System3.DTOs.Request
{
    public class RentalRecordUpdateRequest
    {
        public Guid Id { get; set; }
        public decimal? Payment { get; set; }
    }
}
