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

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasProductsInCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
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

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Guid? categoryId = null, 
        bool? isActive = null, 
        bool? inStockOnly = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null, 
        string? sortBy = null, 
        bool sortDescending = false, 
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(p => p.Category).AsQueryable();

        // Apply filters
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (inStockOnly.HasValue && inStockOnly.Value)
            query = query.Where(p => p.StockQuantity > 0);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price.Amount <= maxPrice.Value);

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> SearchPagedAsync(
        string searchTerm, 
        int pageNumber, 
        int pageSize, 
        Guid? categoryId = null, 
        bool? isActive = null, 
        bool? inStockOnly = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null, 
        string? sortBy = null, 
        bool sortDescending = false, 
        CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        var query = DbSet.Include(p => p.Category)
            .Where(p => p.NameEn.ToLower().Contains(lowerSearchTerm) ||
                       p.NameAr.ToLower().Contains(lowerSearchTerm) ||
                       p.DescriptionEn.ToLower().Contains(lowerSearchTerm) ||
                       p.DescriptionAr.ToLower().Contains(lowerSearchTerm) ||
                       p.ProductSku.ToLower().Contains(lowerSearchTerm));

        // Apply filters
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (inStockOnly.HasValue && inStockOnly.Value)
            query = query.Where(p => p.StockQuantity > 0);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price.Amount <= maxPrice.Value);

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return sortDescending 
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt);
        }

        return sortBy.ToLower() switch
        {
            "name" => sortDescending 
                ? query.OrderByDescending(p => p.NameEn)
                : query.OrderBy(p => p.NameEn),
            "price" => sortDescending 
                ? query.OrderByDescending(p => p.Price.Amount)
                : query.OrderBy(p => p.Price.Amount),
            "stock" => sortDescending 
                ? query.OrderByDescending(p => p.StockQuantity)
                : query.OrderBy(p => p.StockQuantity),
            "created" => sortDescending 
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            "updated" => sortDescending 
                ? query.OrderByDescending(p => p.UpdatedAt)
                : query.OrderBy(p => p.UpdatedAt),
            _ => sortDescending 
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt)
        };
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

    public async Task<IEnumerable<Product>> GetAllWithCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
    }
}