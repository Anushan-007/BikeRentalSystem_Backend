using BikeRental_System3.Enus;

namespace BikeRental_System3.Models
{
    public class EmailTemplate
    {
        public Guid Id { get; set; }
        public EmailTypes emailTypes { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
