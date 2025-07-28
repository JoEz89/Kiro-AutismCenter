using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Features.Orders.Commands.Admin.ProcessRefundAdmin;

public class ProcessRefundAdminHandler : IRequestHandler<ProcessRefundAdminCommand, ProcessRefundAdminResponse>
{
    private readonly IOrderRepository _orderRepository;

    public ProcessRefundAdminHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ProcessRefundAdminResponse> Handle(ProcessRefundAdminCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException($"Order with ID {request.OrderId} not found");
        }

        // Validate refund amount
        var refundAmount = request.RefundAmount ?? order.TotalAmount.Amount;
        if (refundAmount <= 0 || refundAmount > order.TotalAmount.Amount)
        {
            throw new ValidationException($"Invalid refund amount. Must be between 0 and {order.TotalAmount.Amount}");
        }

        // Process the refund
        order.ProcessRefund();

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return new ProcessRefundAdminResponse(
            order.Id,
            order.OrderNumber,
            refundAmount,
            order.TotalAmount.Currency,
            order.Status,
            order.PaymentStatus,
            DateTime.UtcNow);
    }
}