namespace BikeRental_System3.Models
{
    /// <summary>
    /// Supported language definition.
    /// Example: { Code="ta", Name="Tamil", NativeName="தமிழ்" }
    /// </summary>
    public class Language
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;        // "en", "ta", "si"
        public string Name { get; set; } = string.Empty;        // "English", "Tamil"
        public string NativeName { get; set; } = string.Empty;  // "தமிழ்", "සිංහල"
        public string Direction { get; set; } = "ltr";          // "ltr" or "rtl"
        public bool IsActive { get; set; } = true;

        public ICollection<Translation> Translations { get; set; } = new List<Translation>();
    }
}
