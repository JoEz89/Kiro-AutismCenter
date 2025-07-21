using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Cart.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Cart.Commands.ClearCart;

public class ClearCartHandler : IRequestHandler<ClearCartCommand, ClearCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClearCartHandler(
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClearCartResponse> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        // Get user's cart
        var cart = await _cartRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        if (cart == null)
        {
            throw new InvalidOperationException("Cart not found");
        }

        // Clear cart
        cart.Clear();

        // Update cart
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load cart for response
        var updatedCart = await _cartRepository.GetByIdAsync(cart.Id, cancellationToken);
        
        return new ClearCartResponse(
            CartDto.FromEntity(updatedCart!),
            "Cart cleared successfully"
        );
    }
}