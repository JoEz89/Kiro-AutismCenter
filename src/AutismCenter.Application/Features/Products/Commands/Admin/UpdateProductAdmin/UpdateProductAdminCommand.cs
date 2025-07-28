using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.Admin.UpdateProductAdmin;

public record UpdateProductAdminCommand(
    Guid ProductId,
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    decimal Price,
    string Currency,
    int StockQuantity,
    Guid CategoryId,
    IEnumerable<string>? ImageUrls = null,
    bool IsActive = true
) : IRequest<UpdateProductAdminResponse>;