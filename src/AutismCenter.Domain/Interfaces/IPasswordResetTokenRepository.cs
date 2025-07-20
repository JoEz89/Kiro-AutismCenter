using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<List<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId);
    Task AddAsync(PasswordResetToken token);
    Task UpdateAsync(PasswordResetToken token);
    Task RevokeAllUserTokensAsync(Guid userId);
}