using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Products.Queries.Admin.GetInventoryReport;

public class GetInventoryReportHandler : IRequestHandler<GetInventoryReportQuery, GetInventoryReportResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetInventoryReportHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<GetInventoryReportResponse> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken)
    {
        // Get products with categories
        var products = await _productRepository.GetAllWithCategoriesAsync(cancellationToken);

        // Apply filters
        if (request.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.IsActive.HasValue)
        {
            products = products.Where(p => p.IsActive == request.IsActive.Value);
        }

        if (request.LowStockOnly == true)
        {
            products = products.Where(p => p.StockQuantity <= request.LowStockThreshold);
        }

        var productList = products.ToList();

        // Create inventory report items
        var items = productList.Select(p => new InventoryReportItem(
            p.Id,
            p.NameEn,
            p.NameAr,
            p.ProductSku,
            p.Category.NameEn,
            p.Category.NameAr,
            p.StockQuantity,
            p.Price.Amount,
            p.Price.Currency,
            p.Price.Amount * p.StockQuantity,
            p.IsActive,
            p.StockQuantity <= request.LowStockThreshold,
            p.UpdatedAt));

        // Calculate summary
        var summary = new InventoryReportSummary(
            productList.Count,
            productList.Count(p => p.IsActive),
            productList.Count(p => !p.IsActive),
            productList.Count(p => p.StockQuantity <= request.LowStockThreshold && p.StockQuantity > 0),
            productList.Count(p => p.StockQuantity == 0),
            productList.Sum(p => p.Price.Amount * p.StockQuantity),
            productList.Any() ? (decimal)productList.Average(p => p.StockQuantity) : 0);

        return new GetInventoryReportResponse(items, summary);
    }
}