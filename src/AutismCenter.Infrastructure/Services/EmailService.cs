using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailTemplateService _emailTemplateService;

    public EmailService(
        IConfiguration configuration, 
        ILogger<EmailService> logger,
        IEmailTemplateService emailTemplateService)
    {
        _configuration = configuration;
        _logger = logger;
        _emailTemplateService = emailTemplateService;
    }

    public async Task SendEmailVerificationAsync(string email, string firstName, string verificationToken, string language = "en")
    {
        var lang = language == "ar" ? Language.Arabic : Language.English;
        var verificationUrl = $"{_configuration["AppSettings:BaseUrl"]}/verify-email?token={verificationToken}";
        
        var parameters = new Dictionary<string, object>
        {
            { "firstName", firstName },
            { "verificationUrl", verificationUrl }
        };

        try
        {
            var (subject, body) = await _emailTemplateService.GetEmailTemplateAsync("email_verification", lang);
            
            // Process template with parameters
            var processedSubject = ProcessTemplate(subject, parameters);
            var processedBody = ProcessTemplate(body, parameters);
            
            await SendEmailAsync(email, processedSubject, processedBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to use email template, falling back to hardcoded template");
            
            // Fallback to hardcoded templates
            var subject = language == "ar" ? "تأكيد البريد الإلكتروني" : "Email Verification";
            var body = language == "ar" 
                ? $"مرحباً {firstName},\n\nيرجى النقر على الرابط التالي لتأكيد بريدك الإلكتروني:\n{verificationUrl}\n\nشكراً لك"
                : $"Hello {firstName},\n\nPlease click the following link to verify your email address:\n{verificationUrl}\n\nThank you";

            await SendEmailAsync(email, subject, body);
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName, string language = "en")
    {
        var lang = language == "ar" ? Language.Arabic : Language.English;
        
        var parameters = new Dictionary<string, object>
        {
            { "firstName", firstName }
        };

        try
        {
            var (subject, body) = await _emailTemplateService.GetEmailTemplateAsync("welcome_email", lang);
            
            // Process template with parameters
            var processedSubject = ProcessTemplate(subject, parameters);
            var processedBody = ProcessTemplate(body, parameters);
            
            await SendEmailAsync(email, processedSubject, processedBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to use email template, falling back to hardcoded template");
            
            // Fallback to hardcoded templates
            var subject = language == "ar" ? "مرحباً بك في مركز التوحد" : "Welcome to Autism Center";
            var body = language == "ar"
                ? $"مرحباً {firstName},\n\nمرحباً بك في مركز التوحد! تم تأكيد حسابك بنجاح.\n\nيمكنك الآن الوصول إلى جميع خدماتنا.\n\nشكراً لك"
                : $"Hello {firstName},\n\nWelcome to Autism Center! Your account has been successfully verified.\n\nYou can now access all our services.\n\nThank you";

            await SendEmailAsync(email, subject, body);
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string firstName, string resetToken, string language = "en")
    {
        var lang = language == "ar" ? Language.Arabic : Language.English;
        var resetUrl = $"{_configuration["AppSettings:BaseUrl"]}/reset-password?token={resetToken}";
        
        var parameters = new Dictionary<string, object>
        {
            { "firstName", firstName },
            { "resetUrl", resetUrl }
        };

        try
        {
            var (subject, body) = await _emailTemplateService.GetEmailTemplateAsync("password_reset", lang);
            
            // Process template with parameters
            var processedSubject = ProcessTemplate(subject, parameters);
            var processedBody = ProcessTemplate(body, parameters);
            
            await SendEmailAsync(email, processedSubject, processedBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to use email template, falling back to hardcoded template");
            
            // Fallback to hardcoded templates
            var subject = language == "ar" ? "إعادة تعيين كلمة المرور" : "Password Reset";
            var body = language == "ar"
                ? $"مرحباً {firstName},\n\nيرجى النقر على الرابط التالي لإعادة تعيين كلمة المرور:\n{resetUrl}\n\nإذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد الإلكتروني.\n\nشكراً لك"
                : $"Hello {firstName},\n\nPlease click the following link to reset your password:\n{resetUrl}\n\nIf you did not request a password reset, please ignore this email.\n\nThank you";

            await SendEmailAsync(email, subject, body);
        }
    }

    public async Task SendEmailAsync(string email, string subject, string body, CancellationToken cancellationToken = default)
    {
        // For now, just log the email (in production, integrate with SendGrid, AWS SES, etc.)
        _logger.LogInformation("Sending email to {Email} with subject: {Subject}", email, subject);
        _logger.LogInformation("Email body: {Body}", body);
        
        // Simulate async email sending
        await Task.Delay(100, cancellationToken);
        
        _logger.LogInformation("Email sent successfully to {Email}", email);
    }

    private async Task SendEmailAsync(string email, string subject, string body)
    {
        await SendEmailAsync(email, subject, body, CancellationToken.None);
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