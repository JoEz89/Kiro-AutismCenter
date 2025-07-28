using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Common.Interfaces;

public interface ILocalizationService
{
    Task<string> GetLocalizedContentAsync(string key, Language language, string? defaultValue = null);
    Task<string> GetLocalizedContentAsync(string key, Language language, string category, string? defaultValue = null);
    Task<Dictionary<string, string>> GetLocalizedContentByCategoryAsync(string category, Language language);
    Task<bool> SetLocalizedContentAsync(string key, Language language, string content, string category, string updatedBy, string? description = null);
    Task<bool> UpdateLocalizedContentAsync(string key, Language language, string content, string updatedBy);
    Task<bool> DeleteLocalizedContentAsync(string key, Language language);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<Dictionary<string, Dictionary<Language, string>>> GetAllContentByKeyAsync(string key);
    Task<bool> ContentExistsAsync(string key, Language language);
}