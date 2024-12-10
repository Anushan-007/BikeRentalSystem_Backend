using BikeRental_System3.Enus;

namespace BikeRental_System3.Models
{
    public class SendMailRequest
    {
        public string? Name { get; set; }
        public string? Otp { get; set; }

        public string? Email { get; set; }
        public EmailTypes EmailType { get; set; }
        public RentalRequest? RentalRequest { get; set; }
        public RentalRecord? RentalRecord { get; set; }
    }
}
