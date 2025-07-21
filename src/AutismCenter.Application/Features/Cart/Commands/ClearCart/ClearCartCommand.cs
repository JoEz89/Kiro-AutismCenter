using MediatR;

namespace AutismCenter.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand(
    Guid UserId
) : IRequest<ClearCartResponse>;