namespace BikeRental_System3.DTOs
{
    // ── Request DTOs ──────────────────────────────────────────────────────────

    public class UpsertTranslationDto
    {
        public string Key { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Category { get; set; }
    }

    public class BulkTranslationDto
    {
        public string LanguageCode { get; set; } = string.Empty;
        public Dictionary<string, string> Translations { get; set; } = new();
        public string? Category { get; set; }
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────

    /// <summary>
    /// Full translation bundle returned to Angular ngx-translate.
    /// Angular reads the flat Translations dictionary directly.
    /// </summary>
    public class TranslationBundleDto
    {
        public string LanguageCode { get; set; } = string.Empty;
        public string LanguageName { get; set; } = string.Empty;
        public string Direction { get; set; } = "ltr";
        public Dictionary<string, string> Translations { get; set; } = new();
        public bool FromCache { get; set; }
    }

    public class LanguageDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string Direction { get; set; } = "ltr";
        public bool IsActive { get; set; }
    }
}
