using System.ComponentModel.DataAnnotations;

namespace BikeRental_System3.Models
{
    public class RentalRecord
    {
        [Key]
        public int Id { get; set; }
        public DateTime? RentalOut { get; set; }
        public DateTime? RentalReturn { get; set; }
        public decimal? Payment {  get; set; }
        //public string Feedback { get; set; }
        public int RentalRequestId { get; set; }
        public string? RegistrationNumber { get; set; }
        public RentalRequest? RentalRequest { get; set; }
        public Inventory? inventory { get; set; }
    }
}
