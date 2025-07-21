using MediatR;

namespace AutismCenter.Application.Features.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity
) : IRequest<UpdateCartItemResponse>;