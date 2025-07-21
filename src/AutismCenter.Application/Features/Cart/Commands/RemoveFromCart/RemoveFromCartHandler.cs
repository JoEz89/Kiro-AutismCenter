using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Cart.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Cart.Commands.RemoveFromCart;

public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand, RemoveFromCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromCartHandler(
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RemoveFromCartResponse> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        // Get user's cart
        var cart = await _cartRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        if (cart == null)
        {
            throw new InvalidOperationException("Cart not found");
        }

        // Remove item from cart
        cart.RemoveItem(request.ProductId);

        // Update cart
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load cart with products for response
        var updatedCart = await _cartRepository.GetByIdAsync(cart.Id, cancellationToken);
        
        return new RemoveFromCartResponse(
            CartDto.FromEntity(updatedCart!),
            "Item removed from cart successfully"
        );
    }
}