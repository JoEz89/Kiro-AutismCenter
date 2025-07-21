using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Cart.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, UpdateCartItemResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateCartItemResponse> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        // Get user's cart
        var cart = await _cartRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        if (cart == null)
        {
            throw new InvalidOperationException("Cart not found");
        }

        // Validate product exists if quantity > 0
        if (request.Quantity > 0)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException("Product is not available");
            }

            if (!product.HasSufficientStock(request.Quantity))
            {
                throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}, Requested: {request.Quantity}");
            }
        }

        // Update cart item quantity
        cart.UpdateItemQuantity(request.ProductId, request.Quantity);

        // Update cart
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load cart with products for response
        var updatedCart = await _cartRepository.GetByIdAsync(cart.Id, cancellationToken);
        
        var message = request.Quantity == 0 ? "Item removed from cart" : "Cart item updated successfully";
        
        return new UpdateCartItemResponse(
            CartDto.FromEntity(updatedCart!),
            message
        );
    }
}