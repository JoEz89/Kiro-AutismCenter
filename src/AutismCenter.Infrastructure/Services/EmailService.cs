using AutismCenter.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string email, string firstName, string verificationToken, string language = "en")
    {
        var subject = language == "ar" ? "تأكيد البريد الإلكتروني" : "Email Verification";
        var verificationUrl = $"{_configuration["AppSettings:BaseUrl"]}/verify-email?token={verificationToken}";
        
        var body = language == "ar" 
            ? $"مرحباً {firstName},\n\nيرجى النقر على الرابط التالي لتأكيد بريدك الإلكتروني:\n{verificationUrl}\n\nشكراً لك"
            : $"Hello {firstName},\n\nPlease click the following link to verify your email address:\n{verificationUrl}\n\nThank you";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName, string language = "en")
    {
        var subject = language == "ar" ? "مرحباً بك في مركز التوحد" : "Welcome to Autism Center";
        
        var body = language == "ar"
            ? $"مرحباً {firstName},\n\nمرحباً بك في مركز التوحد! تم تأكيد حسابك بنجاح.\n\nيمكنك الآن الوصول إلى جميع خدماتنا.\n\nشكراً لك"
            : $"Hello {firstName},\n\nWelcome to Autism Center! Your account has been successfully verified.\n\nYou can now access all our services.\n\nThank you";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string firstName, string resetToken, string language = "en")
    {
        var subject = language == "ar" ? "إعادة تعيين كلمة المرور" : "Password Reset";
        var resetUrl = $"{_configuration["AppSettings:BaseUrl"]}/reset-password?token={resetToken}";
        
        var body = language == "ar"
            ? $"مرحباً {firstName},\n\nيرجى النقر على الرابط التالي لإعادة تعيين كلمة المرور:\n{resetUrl}\n\nإذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد الإلكتروني.\n\nشكراً لك"
            : $"Hello {firstName},\n\nPlease click the following link to reset your password:\n{resetUrl}\n\nIf you did not request a password reset, please ignore this email.\n\nThank you";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string email, string subject, string body)
    {
        // For now, just log the email (in production, integrate with SendGrid, AWS SES, etc.)
        _logger.LogInformation("Sending email to {Email} with subject: {Subject}", email, subject);
        _logger.LogInformation("Email body: {Body}", body);
        
        // Simulate async email sending
        await Task.Delay(100);
        
        _logger.LogInformation("Email sent successfully to {Email}", email);
    }
}