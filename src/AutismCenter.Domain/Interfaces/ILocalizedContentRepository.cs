using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Domain.Interfaces;

public interface ILocalizedContentRepository
{
    Task<LocalizedContent?> GetByKeyAndLanguageAsync(string key, Language language);
    Task<IEnumerable<LocalizedContent>> GetByCategoryAsync(string category, Language? language = null);
    Task<IEnumerable<LocalizedContent>> GetAllAsync(Language? language = null);
    Task<IEnumerable<LocalizedContent>> GetActiveAsync(Language? language = null);
    Task<LocalizedContent?> GetByIdAsync(Guid id);
    Task AddAsync(LocalizedContent localizedContent);
    Task UpdateAsync(LocalizedContent localizedContent);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string key, Language language);
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<LocalizedContent>> SearchAsync(string searchTerm, Language? language = null);
}