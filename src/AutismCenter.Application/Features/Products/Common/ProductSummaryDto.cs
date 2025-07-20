using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Products.Common;

public record ProductSummaryDto(
    Guid Id,
    string NameEn,
    string NameAr,
    decimal Price,
    string Currency,
    int StockQuantity,
    string CategoryNameEn,
    string CategoryNameAr,
    bool IsActive,
    string ProductSku,
    string? PrimaryImageUrl
)
{
    public static ProductSummaryDto FromEntity(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        return new ProductSummaryDto(
            product.Id,
            product.NameEn ?? string.Empty,
            product.NameAr ?? string.Empty,
            product.Price?.Amount ?? 0,
            product.Price?.Currency ?? "USD",
            product.StockQuantity,
            product.Category?.NameEn ?? string.Empty,
            product.Category?.NameAr ?? string.Empty,
            product.IsActive,
            product.ProductSku ?? string.Empty,
            product.ImageUrls?.FirstOrDefault()
        );
    }

    public string GetName(bool isArabic) => isArabic ? NameAr : NameEn;
    public string GetCategoryName(bool isArabic) => isArabic ? CategoryNameAr : CategoryNameEn;
    public bool IsInStock => StockQuantity > 0;
}