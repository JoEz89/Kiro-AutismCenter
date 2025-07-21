using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Cart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cart>> GetExpiredCartsAsync(CancellationToken cancellationToken = default);
    Task DeleteExpiredCartsAsync(CancellationToken cancellationToken = default);
}