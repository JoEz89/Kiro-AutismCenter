using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Infrastructure.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ILocalizedContentRepository _localizedContentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LocalizationService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public LocalizationService(
        ILocalizedContentRepository localizedContentRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<LocalizationService> logger)
    {
        _localizedContentRepository = localizedContentRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetLocalizedContentAsync(string key, Language language, string? defaultValue = null)
    {
        var cacheKey = $"localized_content_{key}_{language}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedContent) && cachedContent != null)
        {
            return cachedContent;
        }

        try
        {
            var content = await _localizedContentRepository.GetByKeyAndLanguageAsync(key, language);
            var result = content?.Content ?? defaultValue ?? key;
            
            _cache.Set(cacheKey, result, _cacheExpiration);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localized content for key {Key} and language {Language}", key, language);
            return defaultValue ?? key;
        }
    }

    public async Task<string> GetLocalizedContentAsync(string key, Language language, string category, string? defaultValue = null)
    {
        var cacheKey = $"localized_content_{key}_{language}_{category}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedContent) && cachedContent != null)
        {
            return cachedContent;
        }

        try
        {
            var content = await _localizedContentRepository.GetByKeyAndLanguageAsync(key, language);
            
            // If content exists and matches the category, return it
            if (content != null && content.Category == category)
            {
                var result = content.Content;
                _cache.Set(cacheKey, result, _cacheExpiration);
                return result;
            }
            
            // If not found or category doesn't match, return default
            var defaultResult = defaultValue ?? key;
            _cache.Set(cacheKey, defaultResult, _cacheExpiration);
            return defaultResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localized content for key {Key}, language {Language}, and category {Category}", key, language, category);
            return defaultValue ?? key;
        }
    }

    public async Task<Dictionary<string, string>> GetLocalizedContentByCategoryAsync(string category, Language language)
    {
        var cacheKey = $"localized_content_category_{category}_{language}";
        
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cachedContent) && cachedContent != null)
        {
            return cachedContent;
        }

        try
        {
            var contents = await _localizedContentRepository.GetByCategoryAsync(category, language);
            var result = contents
                .Where(c => c.IsActive)
                .ToDictionary(c => c.Key, c => c.Content);
            
            _cache.Set(cacheKey, result, _cacheExpiration);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localized content for category {Category} and language {Language}", category, language);
            return new Dictionary<string, string>();
        }
    }

    public async Task<bool> SetLocalizedContentAsync(string key, Language language, string content, string category, string updatedBy, string? description = null)
    {
        try
        {
            var existingContent = await _localizedContentRepository.GetByKeyAndLanguageAsync(key, language);
            
            if (existingContent != null)
            {
                existingContent.UpdateContent(content, updatedBy);
                if (description != null)
                {
                    existingContent.UpdateDescription(description, updatedBy);
                }
                await _localizedContentRepository.UpdateAsync(existingContent);
            }
            else
            {
                var newContent = new LocalizedContent(key, language, content, category, description, updatedBy);
                await _localizedContentRepository.AddAsync(newContent);
            }

            await _unitOfWork.SaveChangesAsync();
            
            // Clear cache
            var cacheKey = $"localized_content_{key}_{language}";
            _cache.Remove(cacheKey);
            _cache.Remove($"localized_content_category_{category}_{language}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting localized content for key {Key} and language {Language}", key, language);
            return false;
        }
    }

    public async Task<bool> UpdateLocalizedContentAsync(string key, Language language, string content, string updatedBy)
    {
        try
        {
            var existingContent = await _localizedContentRepository.GetByKeyAndLanguageAsync(key, language);
            
            if (existingContent == null)
            {
                return false;
            }

            existingContent.UpdateContent(content, updatedBy);
            await _localizedContentRepository.UpdateAsync(existingContent);
            await _unitOfWork.SaveChangesAsync();
            
            // Clear cache
            var cacheKey = $"localized_content_{key}_{language}";
            _cache.Remove(cacheKey);
            _cache.Remove($"localized_content_category_{existingContent.Category}_{language}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating localized content for key {Key} and language {Language}", key, language);
            return false;
        }
    }

    public async Task<bool> DeleteLocalizedContentAsync(string key, Language language)
    {
        try
        {
            var content = await _localizedContentRepository.GetByKeyAndLanguageAsync(key, language);
            
            if (content == null)
            {
                return false;
            }

            await _localizedContentRepository.DeleteAsync(content.Id);
            await _unitOfWork.SaveChangesAsync();
            
            // Clear cache
            var cacheKey = $"localized_content_{key}_{language}";
            _cache.Remove(cacheKey);
            _cache.Remove($"localized_content_category_{content.Category}_{language}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting localized content for key {Key} and language {Language}", key, language);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        try
        {
            return await _localizedContentRepository.GetCategoriesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<Dictionary<string, Dictionary<Language, string>>> GetAllContentByKeyAsync(string key)
    {
        try
        {
            var allContent = await _localizedContentRepository.GetAllAsync();
            var keyContent = allContent.Where(c => c.Key == key);
            
            var result = new Dictionary<string, Dictionary<Language, string>>();
            
            foreach (var content in keyContent)
            {
                if (!result.ContainsKey(content.Key))
                {
                    result[content.Key] = new Dictionary<Language, string>();
                }
                result[content.Key][content.Language] = content.Content;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all content for key {Key}", key);
            return new Dictionary<string, Dictionary<Language, string>>();
        }
    }

    public async Task<bool> ContentExistsAsync(string key, Language language)
    {
        try
        {
            return await _localizedContentRepository.ExistsAsync(key, language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if content exists for key {Key} and language {Language}", key, language);
            return false;
        }
    }
}