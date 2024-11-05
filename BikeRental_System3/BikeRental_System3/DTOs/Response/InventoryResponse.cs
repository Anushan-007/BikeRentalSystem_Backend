namespace BikeRental_System3.DTOs.Response
{
    public class InventoryResponse
    {
        public string RegistrationNumber { get; set; }
        public int YearofManufacture { get; set; }
        public bool Availability { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsDeleted { get; set; }
        public int BikeId { get; set; }
    }
}
