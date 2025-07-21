using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Cart.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Cart.Commands.AddToCart;

public class AddToCartHandler : IRequestHandler<AddToCartCommand, AddToCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddToCartHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddToCartResponse> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Validate product exists and is available
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

        // Get or create cart for user
        var cart = await _cartRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        if (cart == null)
        {
            cart = Domain.Entities.Cart.Create(request.UserId);
            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        // Add item to cart
        cart.AddItem(request.ProductId, request.Quantity, product.Price);

        // Update cart
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load cart with products for response
        var updatedCart = await _cartRepository.GetByIdAsync(cart.Id, cancellationToken);
        
        return new AddToCartResponse(
            CartDto.FromEntity(updatedCart!),
            "Item added to cart successfully"
        );
    }
}