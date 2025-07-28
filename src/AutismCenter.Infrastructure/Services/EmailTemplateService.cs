using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AutismCenter.Infrastructure.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public EmailTemplateService(
        IEmailTemplateRepository emailTemplateRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<EmailTemplateService> logger)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<(string Subject, string Body)> GetEmailTemplateAsync(string templateKey, Language language)
    {
        var cacheKey = $"email_template_{templateKey}_{language}";
        
        if (_cache.TryGetValue(cacheKey, out (string Subject, string Body) cachedTemplate))
        {
            return cachedTemplate;
        }

        try
        {
            var template = await _emailTemplateRepository.GetByTemplateKeyAndLanguageAsync(templateKey, language);
            
            if (template != null && template.IsActive)
            {
                var result = (template.Subject, template.Body);
                _cache.Set(cacheKey, result, _cacheExpiration);
                return result;
            }
            
            // Fallback to English if Arabic not found
            if (language == Language.Arabic)
            {
                var englishTemplate = await _emailTemplateRepository.GetByTemplateKeyAndLanguageAsync(templateKey, Language.English);
                if (englishTemplate != null && englishTemplate.IsActive)
                {
                    var result = (englishTemplate.Subject, englishTemplate.Body);
                    _cache.Set(cacheKey, result, _cacheExpiration);
                    return result;
                }
            }
            
            // Return default template if not found
            var defaultResult = ($"Subject for {templateKey}", $"Body for {templateKey}");
            _cache.Set(cacheKey, defaultResult, _cacheExpiration);
            return defaultResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email template for key {TemplateKey} and language {Language}", templateKey, language);
            return ($"Subject for {templateKey}", $"Body for {templateKey}");
        }
    }

    public async Task<string> ProcessEmailTemplateAsync(string templateKey, Language language, Dictionary<string, object> parameters)
    {
        try
        {
            var (subject, body) = await GetEmailTemplateAsync(templateKey, language);
            
            var processedSubject = ProcessTemplate(subject, parameters);
            var processedBody = ProcessTemplate(body, parameters);
            
            return $"Subject: {processedSubject}\n\nBody:\n{processedBody}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email template for key {TemplateKey} and language {Language}", templateKey, language);
            return $"Error processing template {templateKey}";
        }
    }

    public async Task<bool> SetEmailTemplateAsync(string templateKey, Language language, string subject, string body, string updatedBy, string? description = null)
    {
        try
        {
            var existingTemplate = await _emailTemplateRepository.GetByTemplateKeyAndLanguageAsync(templateKey, language);
            
            if (existingTemplate != null)
            {
                existingTemplate.UpdateTemplate(subject, body, updatedBy);
                if (description != null)
                {
                    existingTemplate.UpdateDescription(description, updatedBy);
                }
                await _emailTemplateRepository.UpdateAsync(existingTemplate);
            }
            else
            {
                var newTemplate = new EmailTemplate(templateKey, language, subject, body, description, updatedBy);
                await _emailTemplateRepository.AddAsync(newTemplate);
            }

            await _unitOfWork.SaveChangesAsync();
            
            // Clear cache
            var cacheKey = $"email_template_{templateKey}_{language}";
            _cache.Remove(cacheKey);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting email template for key {TemplateKey} and language {Language}", templateKey, language);
            return false;
        }
    }

    public async Task<bool> UpdateEmailTemplateAsync(string templateKey, Language language, string subject, string body, string updatedBy)
    {
        try
        {
            var existingTemplate = await _emailTemplateRepository.GetByTemplateKeyAndLanguageAsync(templateKey, language);
            
            if (existingTemplate == null)
            {
                return false;
            }

            existingTemplate.UpdateTemplate(subject, body, updatedBy);
            await _emailTemplateRepository.UpdateAsync(existingTemplate);
            await _unitOfWork.SaveChangesAsync();
            
            // Clear cache
            var cacheKey = $"email_template_{templateKey}_{language}";
            _cache.Remove(cacheKey);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email template for key {TemplateKey} and language {Language}", templateKey, language);
            return false;
        }
    }

    public async Task<bool> DeleteEmailTemplateAsync(string templateKey, Language language)
    {
        try
        {
            var template = await _emailTemplateRepository.GetByTemplateKeyAndLanguageAsync(templateKey, language);
            
            if (template == null)
            {
                return false;
            }

            await _emailTemplateRepository.DeleteAsync(template.Id);
            await _unitOfWork.SaveChangesAsync();
            
            // Clear cache
            var cacheKey = $"email_template_{templateKey}_{language}";
            _cache.Remove(cacheKey);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email template for key {TemplateKey} and language {Language}", templateKey, language);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetTemplateKeysAsync()
    {
        try
        {
            return await _emailTemplateRepository.GetTemplateKeysAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template keys");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> TemplateExistsAsync(string templateKey, Language language)
    {
        try
        {
            return await _emailTemplateRepository.ExistsAsync(templateKey, language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if template exists for key {TemplateKey} and language {Language}", templateKey, language);
            return false;
        }
    }

    private static string ProcessTemplate(string template, Dictionary<string, object> parameters)
    {
        if (string.IsNullOrEmpty(template) || parameters == null || !parameters.Any())
        {
            return template;
        }

        var result = template;
        
        // Replace placeholders in the format {{key}} with parameter values
        foreach (var parameter in parameters)
        {
            var placeholder = $"{{{{{parameter.Key}}}}}";
            var value = parameter.Value?.ToString() ?? string.Empty;
            result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        // Also support {key} format for backward compatibility
        foreach (var parameter in parameters)
        {
            var placeholder = $"{{{parameter.Key}}}";
            var value = parameter.Value?.ToString() ?? string.Empty;
            result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}