using MediatR;

namespace AutismCenter.Application.Features.Cart.Queries.GetCart;

public record GetCartQuery(
    Guid UserId
) : IRequest<GetCartResponse>;