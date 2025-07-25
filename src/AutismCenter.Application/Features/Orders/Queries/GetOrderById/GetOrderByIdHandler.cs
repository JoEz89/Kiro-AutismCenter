using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        
        if (order == null)
            return null;

        // If UserId is provided, verify the user owns the order (for user-specific access)
        if (request.UserId.HasValue && order.UserId != request.UserId.Value)
            throw new UnauthorizedAccessException("User is not authorized to view this order");

        return OrderDto.FromEntity(order);
    }
}