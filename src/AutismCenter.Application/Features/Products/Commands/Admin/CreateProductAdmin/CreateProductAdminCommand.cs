using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.Admin.CreateProductAdmin;

public record CreateProductAdminCommand(
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    decimal Price,
    string Currency,
    int StockQuantity,
    Guid CategoryId,
    string ProductSku,
    IEnumerable<string>? ImageUrls = null,
    bool IsActive = true
) : IRequest<CreateProductAdminResponse>;