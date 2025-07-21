using AutismCenter.Application.Features.Cart.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Cart.Queries.GetCart;

public class GetCartHandler : IRequestHandler<GetCartQuery, GetCartResponse>
{
    private readonly ICartRepository _cartRepository;

    public GetCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<GetCartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        
        return new GetCartResponse(
            cart != null ? CartDto.FromEntity(cart) : null
        );
    }
}