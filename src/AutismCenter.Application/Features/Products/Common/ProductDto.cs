using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Products.Common;

public record ProductDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    decimal Price,
    string Currency,
    int StockQuantity,
    Guid CategoryId,
    string CategoryNameEn,
    string CategoryNameAr,
    bool IsActive,
    string ProductSku,
    IEnumerable<string> ImageUrls,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static ProductDto FromEntity(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        return new ProductDto(
            product.Id,
            product.NameEn ?? string.Empty,
            product.NameAr ?? string.Empty,
            product.DescriptionEn ?? string.Empty,
            product.DescriptionAr ?? string.Empty,
            product.Price?.Amount ?? 0,
            product.Price?.Currency ?? "USD",
            product.StockQuantity,
            product.CategoryId,
            product.Category?.NameEn ?? string.Empty,
            product.Category?.NameAr ?? string.Empty,
            product.IsActive,
            product.ProductSku ?? string.Empty,
            product.ImageUrls ?? new List<string>(),
            product.CreatedAt,
            product.UpdatedAt
        );
    }

    public string GetName(bool isArabic) => isArabic ? NameAr : NameEn;
    public string GetDescription(bool isArabic) => isArabic ? DescriptionAr : DescriptionEn;
    public string GetCategoryName(bool isArabic) => isArabic ? CategoryNameAr : CategoryNameEn;
}