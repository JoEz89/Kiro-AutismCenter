using AutismCenter.Application.Features.Cart.Common;

namespace AutismCenter.Application.Features.Cart.Queries.GetCart;

public record GetCartResponse(
    CartDto? Cart
);