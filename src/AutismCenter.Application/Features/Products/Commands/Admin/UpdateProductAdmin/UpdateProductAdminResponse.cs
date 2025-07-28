namespace AutismCenter.Application.Features.Products.Commands.Admin.UpdateProductAdmin;

public record UpdateProductAdminResponse(
    Guid Id,
    string NameEn,
    string NameAr,
    string ProductSku,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    DateTime UpdatedAt
);