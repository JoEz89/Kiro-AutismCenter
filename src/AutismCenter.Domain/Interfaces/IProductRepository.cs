using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Guid? categoryId = null, 
        bool? isActive = null, 
        bool? inStockOnly = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null, 
        string? sortBy = null, 
        bool sortDescending = false, 
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<Product> Products, int TotalCount)> SearchPagedAsync(
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
        CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllWithCategoriesAsync(CancellationToken cancellationToken = default);
}