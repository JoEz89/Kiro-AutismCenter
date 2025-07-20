namespace AutismCenter.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string firstName, string verificationToken, string language = "en");
    Task SendWelcomeEmailAsync(string email, string firstName, string language = "en");
    Task SendPasswordResetEmailAsync(string email, string firstName, string resetToken, string language = "en");
}