using AutismCenter.Application.Features.Cart.Common;

namespace AutismCenter.Application.Features.Cart.Commands.RemoveFromCart;

public record RemoveFromCartResponse(
    CartDto Cart,
    string Message
);