namespace AutismCenter.Application.Features.Products.Commands.Admin.CreateProductAdmin;

public record CreateProductAdminResponse(
    Guid Id,
    string NameEn,
    string NameAr,
    string ProductSku,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt
);