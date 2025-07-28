using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Domain.Interfaces;

public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByTemplateKeyAndLanguageAsync(string templateKey, Language language);
    Task<IEnumerable<EmailTemplate>> GetAllAsync(Language? language = null);
    Task<IEnumerable<EmailTemplate>> GetActiveAsync(Language? language = null);
    Task<EmailTemplate?> GetByIdAsync(Guid id);
    Task AddAsync(EmailTemplate emailTemplate);
    Task UpdateAsync(EmailTemplate emailTemplate);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string templateKey, Language language);
    Task<IEnumerable<string>> GetTemplateKeysAsync();
}