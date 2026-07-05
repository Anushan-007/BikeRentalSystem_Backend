using BikeRental_System3.DTOs;
using BikeRental_System3.IService;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    /// <summary>
    /// Public Localization API consumed by Angular ngx-translate.
    ///
    /// GET /api/localization/languages        → language switcher list
    /// GET /api/localization?lang=ta          → full translation bundle
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LocalizationController : ControllerBase
    {
        private readonly ILocalizationService _svc;

        public LocalizationController(ILocalizationService svc) => _svc = svc;

        // GET /api/localization/languages
        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _svc.GetLanguagesAsync();
            return Ok(languages);
        }

        // GET /api/localization?lang=en
        [HttpGet]
        public async Task<IActionResult> GetTranslations([FromQuery] string lang = "en")
        {
            if (string.IsNullOrWhiteSpace(lang))
                return BadRequest(new { error = "The 'lang' query parameter is required." });

            var bundle = await _svc.GetTranslationsAsync(lang.ToLowerInvariant());

            if (bundle is null)
                return NotFound(new { error = $"Language '{lang}' is not supported." });

            return Ok(bundle);
        }
    }

    /// <summary>
    /// Admin API — add/edit/delete translations at runtime.
    /// Changes go live immediately (cache is invalidated on every save).
    ///
    /// POST   /api/localizationadmin/translations          → add/edit single key
    /// POST   /api/localizationadmin/translations/bulk     → bulk import
    /// DELETE /api/localizationadmin/translations/{key}    → delete key
    /// POST   /api/localizationadmin/cache/invalidate/{lang} → manual flush
    /// </summary>
    [Route("api/localizationadmin")]
    [ApiController]
    public class LocalizationAdminController : ControllerBase
    {
        private readonly ILocalizationService _svc;

        public LocalizationAdminController(ILocalizationService svc) => _svc = svc;

        [HttpPost("translations")]
        public async Task<IActionResult> UpsertTranslation([FromBody] UpsertTranslationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Key) || string.IsNullOrWhiteSpace(dto.LanguageCode))
                return BadRequest(new { error = "Key and LanguageCode are required." });

            var result = await _svc.UpsertTranslationAsync(dto);
            return Ok(result);
        }

        [HttpPost("translations/bulk")]
        public async Task<IActionResult> BulkImport([FromBody] BulkTranslationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LanguageCode) || dto.Translations.Count == 0)
                return BadRequest(new { error = "LanguageCode and translations are required." });

            await _svc.BulkImportTranslationsAsync(dto);
            return Ok(new { message = $"Imported {dto.Translations.Count} translations for '{dto.LanguageCode}'." });
        }

        [HttpDelete("translations/{key}")]
        public async Task<IActionResult> DeleteTranslation(string key)
        {
            var deleted = await _svc.DeleteTranslationKeyAsync(key);
            return deleted
                ? Ok(new { message = $"Key '{key}' deleted." })
                : NotFound(new { error = $"Key '{key}' not found." });
        }

        [HttpPost("cache/invalidate/{lang}")]
        public IActionResult InvalidateCache(string lang)
        {
            _svc.InvalidateCache(lang.ToLowerInvariant());
            return Ok(new { message = $"Cache cleared for '{lang}'." });
        }
    }
}
