using BikeRental_System3.Data;
using BikeRental_System3.DTOs;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BikeRental_System3.Services
{
    /// <summary>
    /// Localization Service — Cache-Aside Pattern
    ///
    /// Flow:
    ///   Angular requests translations
    ///       → Check MemoryCache
    ///       → Cache HIT  : return immediately (fast)
    ///       → Cache MISS : query DB → store in cache → return
    ///
    /// When admin edits a translation, cache is invalidated instantly
    /// so changes are live without restarting the app.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<LocalizationService> _logger;

        private const string CacheKeyPrefix = "translations_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public LocalizationService(AppDbContext db, IMemoryCache cache, ILogger<LocalizationService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        // ── Languages ─────────────────────────────────────────────────────────

        public async Task<IEnumerable<LanguageDto>> GetLanguagesAsync()
        {
            return await _db.Languages
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LanguageDto
                {
                    Id         = l.Id,
                    Code       = l.Code,
                    Name       = l.Name,
                    NativeName = l.NativeName,
                    Direction  = l.Direction,
                    IsActive   = l.IsActive
                })
                .ToListAsync();
        }

        // ── Translations ──────────────────────────────────────────────────────

        public async Task<TranslationBundleDto?> GetTranslationsAsync(string languageCode)
        {
            var lang = await _db.Languages
                .FirstOrDefaultAsync(l => l.Code == languageCode && l.IsActive);

            if (lang is null) return null;

            var cacheKey = CacheKeyPrefix + languageCode;
            bool fromCache = _cache.TryGetValue(cacheKey, out Dictionary<string, string>? cached);

            if (!fromCache || cached is null)
            {
                _logger.LogInformation("Cache MISS for '{Lang}' — loading from DB.", languageCode);

                cached = await _db.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .ToDictionaryAsync(t => t.Key, t => t.Value);

                _cache.Set(cacheKey, cached, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CacheDuration));
            }
            else
            {
                _logger.LogInformation("Cache HIT for '{Lang}'.", languageCode);
            }

            return new TranslationBundleDto
            {
                LanguageCode = lang.Code,
                LanguageName = lang.Name,
                Direction    = lang.Direction,
                Translations = cached,
                FromCache    = fromCache
            };
        }

        public async Task<Translation> UpsertTranslationAsync(UpsertTranslationDto dto)
        {
            var existing = await _db.Translations
                .FirstOrDefaultAsync(t => t.Key == dto.Key && t.LanguageCode == dto.LanguageCode);

            if (existing is null)
            {
                existing = new Translation();
                _db.Translations.Add(existing);
            }

            existing.Key          = dto.Key.ToUpperInvariant();
            existing.LanguageCode = dto.LanguageCode;
            existing.Value        = dto.Value;
            existing.Category     = dto.Category;
            existing.UpdatedAt    = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            InvalidateCache(dto.LanguageCode);
            return existing;
        }

        public async Task BulkImportTranslationsAsync(BulkTranslationDto dto)
        {
            foreach (var (key, value) in dto.Translations)
            {
                var existing = await _db.Translations
                    .FirstOrDefaultAsync(t => t.Key == key.ToUpperInvariant() && t.LanguageCode == dto.LanguageCode);

                if (existing is null)
                    _db.Translations.Add(new Translation { Key = key.ToUpperInvariant(), LanguageCode = dto.LanguageCode, Value = value, Category = dto.Category, UpdatedAt = DateTime.UtcNow });
                else
                { existing.Value = value; existing.UpdatedAt = DateTime.UtcNow; }
            }

            await _db.SaveChangesAsync();
            InvalidateCache(dto.LanguageCode);
        }

        public async Task<bool> DeleteTranslationKeyAsync(string key)
        {
            var entries = await _db.Translations
                .Where(t => t.Key == key.ToUpperInvariant()).ToListAsync();

            if (entries.Count == 0) return false;

            var langs = entries.Select(e => e.LanguageCode).Distinct().ToList();
            _db.Translations.RemoveRange(entries);
            await _db.SaveChangesAsync();
            langs.ForEach(InvalidateCache);
            return true;
        }

        public void InvalidateCache(string languageCode)
        {
            _cache.Remove(CacheKeyPrefix + languageCode);
            _logger.LogInformation("Cache invalidated for '{Lang}'.", languageCode);
        }
    }
}
