namespace AutismCenter.Application.Common.Interfaces;

public interface IPasswordResetService
{
    string GenerateResetToken(Guid userId);
    Task<bool> ValidateResetTokenAsync(string token);
    Task<Guid?> GetUserIdFromTokenAsync(string token);
    Task RevokeTokenAsync(string token);
}