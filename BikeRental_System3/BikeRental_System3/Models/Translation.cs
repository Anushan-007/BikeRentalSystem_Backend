namespace BikeRental_System3.Models
{
    /// <summary>
    /// Stores a single translation key-value pair for a language.
    /// Example: { Key="LOGIN", LanguageCode="ta", Value="உள்நுழை" }
    /// </summary>
    public class Translation
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;           // "LOGIN", "BOOKING"
        public string LanguageCode { get; set; } = string.Empty;  // "en", "ta", "si"
        public string Value { get; set; } = string.Empty;         // Translated text
        public string? Category { get; set; }                     // "AUTH", "BOOKING", "ERRORS"
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Language? Language { get; set; }
    }
}
