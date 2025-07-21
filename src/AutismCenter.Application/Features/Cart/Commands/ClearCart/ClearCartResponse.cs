using AutismCenter.Application.Features.Cart.Common;

namespace AutismCenter.Application.Features.Cart.Commands.ClearCart;

public record ClearCartResponse(
    CartDto Cart,
    string Message
);