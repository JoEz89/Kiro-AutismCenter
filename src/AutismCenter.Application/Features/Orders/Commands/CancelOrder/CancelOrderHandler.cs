using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        // If UserId is provided, verify the user owns the order
        if (request.UserId.HasValue && order.UserId != request.UserId.Value)
            throw new UnauthorizedAccessException("User is not authorized to cancel this order");

        // Check if order can be cancelled
        if (!order.CanBeCancelled())
            throw new InvalidOperationException($"Order with status {order.Status} cannot be cancelled");

        // Restore product stock for cancelled orders
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product != null)
            {
                product.RestoreStock(item.Quantity);
                await _productRepository.UpdateAsync(product, cancellationToken);
            }
        }

        // Cancel the order
        order.Cancel();

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OrderDto.FromEntity(order);
    }
}