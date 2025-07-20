using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    decimal Price,
    string Currency,
    int StockQuantity,
    Guid CategoryId,
    string ProductSku,
    IEnumerable<string>? ImageUrls = null
) : IRequest<CreateProductResponse>;