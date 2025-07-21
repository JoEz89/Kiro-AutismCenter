using MediatR;

namespace AutismCenter.Application.Features.Cart.Queries.GetCartItemCount;

public record GetCartItemCountQuery(
    Guid UserId
) : IRequest<GetCartItemCountResponse>;