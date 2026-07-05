using BikeRental_System3.DTOs;
using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    public interface ILocalizationService
    {
        Task<IEnumerable<LanguageDto>> GetLanguagesAsync();
        Task<TranslationBundleDto?> GetTranslationsAsync(string languageCode);
        Task<Translation> UpsertTranslationAsync(UpsertTranslationDto dto);
        Task BulkImportTranslationsAsync(BulkTranslationDto dto);
        Task<bool> DeleteTranslationKeyAsync(string key);
        void InvalidateCache(string languageCode);
    }
}
