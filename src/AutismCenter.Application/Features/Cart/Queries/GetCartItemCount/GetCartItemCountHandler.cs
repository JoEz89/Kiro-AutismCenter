using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Cart.Queries.GetCartItemCount;

public class GetCartItemCountHandler : IRequestHandler<GetCartItemCountQuery, GetCartItemCountResponse>
{
    private readonly ICartRepository _cartRepository;

    public GetCartItemCountHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<GetCartItemCountResponse> Handle(GetCartItemCountQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        
        var itemCount = cart?.GetTotalItemCount() ?? 0;
        
        return new GetCartItemCountResponse(itemCount);
    }
}