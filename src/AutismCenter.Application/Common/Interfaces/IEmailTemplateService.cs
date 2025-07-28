using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Common.Interfaces;

public interface IEmailTemplateService
{
    Task<(string Subject, string Body)> GetEmailTemplateAsync(string templateKey, Language language);
    Task<string> ProcessEmailTemplateAsync(string templateKey, Language language, Dictionary<string, object> parameters);
    Task<bool> SetEmailTemplateAsync(string templateKey, Language language, string subject, string body, string updatedBy, string? description = null);
    Task<bool> UpdateEmailTemplateAsync(string templateKey, Language language, string subject, string body, string updatedBy);
    Task<bool> DeleteEmailTemplateAsync(string templateKey, Language language);
    Task<IEnumerable<string>> GetTemplateKeysAsync();
    Task<bool> TemplateExistsAsync(string templateKey, Language language);
}