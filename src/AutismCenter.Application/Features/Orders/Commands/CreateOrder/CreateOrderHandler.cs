using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Application.Features.Orders.Services;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrderNumberService _orderNumberService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IOrderNumberService orderNumberService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _orderNumberService = orderNumberService;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {request.UserId} not found");

        // Validate all products exist and have sufficient stock
        var productValidations = new List<(Product Product, int RequestedQuantity)>();
        
        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {item.ProductId} not found");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product {product.GetName(false)} is not available");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.GetName(false)}. Available: {product.StockQuantity}, Requested: {item.Quantity}");

            productValidations.Add((product, item.Quantity));
        }

        // Generate unique order number
        var orderNumber = await _orderNumberService.GenerateOrderNumberAsync(cancellationToken);

        // Create order
        var shippingAddress = request.ShippingAddress.ToValueObject();
        var billingAddress = request.BillingAddress.ToValueObject();
        
        var order = Order.Create(request.UserId, shippingAddress, billingAddress, orderNumber);

        // Add items to order
        foreach (var (product, quantity) in productValidations)
        {
            order.AddItem(product, quantity, product.Price);
        }

        // Reduce product stock
        foreach (var (product, quantity) in productValidations)
        {
            product.ReduceStock(quantity);
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        // Save order
        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderDto.FromEntity(order);
    }
}