using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    decimal Price,
    string Currency,
    IEnumerable<string>? ImageUrls = null
) : IRequest<UpdateProductResponse>;