using AutismCenter.Application.Features.Products.Common;

namespace AutismCenter.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductResponse(
    ProductDto Product,
    string Message
);