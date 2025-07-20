namespace AutismCenter.Application.Common.Interfaces;

public interface IEmailVerificationService
{
    string GenerateVerificationToken(Guid userId);
    Task<bool> ValidateVerificationTokenAsync(string token);
    Task<Guid?> GetUserIdFromTokenAsync(string token);
    Task RevokeTokenAsync(string token);
}