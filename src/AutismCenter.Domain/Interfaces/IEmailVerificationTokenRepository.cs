using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken?> GetByTokenAsync(string token);
    Task<List<EmailVerificationToken>> GetActiveTokensByUserIdAsync(Guid userId);
    Task AddAsync(EmailVerificationToken token);
    Task UpdateAsync(EmailVerificationToken token);
    Task RevokeAllUserTokensAsync(Guid userId);
}