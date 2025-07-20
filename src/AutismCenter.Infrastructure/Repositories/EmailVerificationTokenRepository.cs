using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutismCenter.Infrastructure.Repositories;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly ApplicationDbContext _context;

    public EmailVerificationTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailVerificationToken?> GetByTokenAsync(string token)
    {
        return await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<List<EmailVerificationToken>> GetActiveTokensByUserIdAsync(Guid userId)
    {
        return await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task AddAsync(EmailVerificationToken token)
    {
        await _context.EmailVerificationTokens.AddAsync(token);
    }

    public Task UpdateAsync(EmailVerificationToken token)
    {
        _context.EmailVerificationTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId);
        
        foreach (var token in activeTokens)
        {
            token.MarkAsUsed();
        }

        if (activeTokens.Any())
        {
            _context.EmailVerificationTokens.UpdateRange(activeTokens);
        }
    }
}