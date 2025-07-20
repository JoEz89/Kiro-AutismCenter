using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductSku == sku, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive && 
                       (p.NameEn.ToLower().Contains(lowerSearchTerm) ||
                        p.NameAr.ToLower().Contains(lowerSearchTerm) ||
                        p.DescriptionEn.ToLower().Contains(lowerSearchTerm) ||
                        p.DescriptionAr.ToLower().Contains(lowerSearchTerm) ||
                        p.ProductSku.ToLower().Contains(lowerSearchTerm)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity <= threshold)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(p => p.ProductSku == sku, cancellationToken);
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
    }
}