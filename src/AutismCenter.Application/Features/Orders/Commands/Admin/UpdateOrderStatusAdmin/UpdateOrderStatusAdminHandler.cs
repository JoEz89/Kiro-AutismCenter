using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Enums;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Orders.Commands.Admin.UpdateOrderStatusAdmin;

public class UpdateOrderStatusAdminHandler : IRequestHandler<UpdateOrderStatusAdminCommand, UpdateOrderStatusAdminResponse>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusAdminHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<UpdateOrderStatusAdminResponse> Handle(UpdateOrderStatusAdminCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException($"Order with ID {request.OrderId} not found");
        }

        // Update order status based on the requested status
        switch (request.Status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Processing:
                order.StartProcessing();
                break;
            case OrderStatus.Shipped:
                order.Ship();
                break;
            case OrderStatus.Delivered:
                order.Deliver();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
            case OrderStatus.Refunded:
                // Note: Refund should typically be handled through payment processing
                // This is just for status tracking
                break;
            default:
                throw new ValidationException($"Invalid order status: {request.Status}");
        }

        // Note: Admin notes functionality can be added to Order entity if needed

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return new UpdateOrderStatusAdminResponse(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.PaymentStatus,
            order.UpdatedAt);
    }
}