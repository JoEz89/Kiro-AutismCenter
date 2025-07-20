using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(u => u.Email.Value == email.Value, cancellationToken);
    }
}