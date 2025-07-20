using AutismCenter.Application.Features.Products.Common;

namespace AutismCenter.Application.Features.Products.Commands.CreateProduct;

public record CreateProductResponse(
    ProductDto Product,
    string Message
);