using AutismCenter.Application.Features.Cart.Common;

namespace AutismCenter.Application.Features.Cart.Commands.UpdateCartItem;

public record UpdateCartItemResponse(
    CartDto Cart,
    string Message
);