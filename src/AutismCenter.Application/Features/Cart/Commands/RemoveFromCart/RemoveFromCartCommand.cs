using MediatR;

namespace AutismCenter.Application.Features.Cart.Commands.RemoveFromCart;

public record RemoveFromCartCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<RemoveFromCartResponse>;