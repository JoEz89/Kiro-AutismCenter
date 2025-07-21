using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.Infrastructure.Repositories;

public class CartRepository : BaseRepository<Cart>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<Cart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId && 
                                    (!c.ExpiresAt.HasValue || c.ExpiresAt.Value > DateTime.UtcNow), 
                                cancellationToken);
    }

    public async Task<IEnumerable<Cart>> GetExpiredCartsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteExpiredCartsAsync(CancellationToken cancellationToken = default)
    {
        var expiredCarts = await GetExpiredCartsAsync(cancellationToken);
        
        if (expiredCarts.Any())
        {
            DbSet.RemoveRange(expiredCarts);
        }
    }

    public override async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Cart>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.User)
            .ToListAsync(cancellationToken);
    }
}