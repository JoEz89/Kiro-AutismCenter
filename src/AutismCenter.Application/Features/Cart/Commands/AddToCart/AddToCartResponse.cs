using AutismCenter.Application.Features.Cart.Common;

namespace AutismCenter.Application.Features.Cart.Commands.AddToCart;

public record AddToCartResponse(
    CartDto Cart,
    string Message
);