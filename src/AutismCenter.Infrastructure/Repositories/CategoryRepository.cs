using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.NameEn)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderBy(c => c.NameEn)
            .ToListAsync(cancellationToken);
    }
}