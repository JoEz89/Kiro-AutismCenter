using AutismCenter.Application.Features.Products.Common;

namespace AutismCenter.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdResponse(
    ProductDto? Product
);