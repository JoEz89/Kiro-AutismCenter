using MediatR;

namespace AutismCenter.Application.Features.Cart.Commands.AddToCart;

public record AddToCartCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity
) : IRequest<AddToCartResponse>;