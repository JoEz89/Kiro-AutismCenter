using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Application.Features.Orders.Commands.RefundPayment;

public class RefundPaymentHandler : IRequestHandler<RefundPaymentCommand, RefundPaymentResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefundPaymentHandler> _logger;

    public RefundPaymentHandler(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<RefundPaymentHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RefundPaymentResult> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new RefundPaymentResult(false, null, "Order not found", Domain.ValueObjects.Money.Create(0));
            }

            if (order.PaymentStatus != Domain.Enums.PaymentStatus.Completed)
            {
                return new RefundPaymentResult(false, null, "Cannot refund order that hasn't been paid", Domain.ValueObjects.Money.Create(0));
            }

            if (string.IsNullOrEmpty(order.PaymentId))
            {
                return new RefundPaymentResult(false, null, "No payment ID found for this order", Domain.ValueObjects.Money.Create(0));
            }

            var refundAmount = request.RefundAmount ?? order.TotalAmount;
            
            var refundResult = await _paymentService.ProcessRefundAsync(order.PaymentId, refundAmount, cancellationToken);

            if (refundResult.IsSuccess)
            {
                order.ProcessRefund();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Refund processed successfully for order {OrderId}, refund ID: {RefundId}, amount: {Amount}", 
                    order.Id, refundResult.RefundId, refundResult.RefundedAmount);

                return new RefundPaymentResult(true, refundResult.RefundId, null, refundResult.RefundedAmount);
            }
            else
            {
                _logger.LogWarning("Refund failed for order {OrderId}: {ErrorMessage}", 
                    order.Id, refundResult.ErrorMessage);

                return new RefundPaymentResult(false, null, refundResult.ErrorMessage ?? "Refund processing failed", Domain.ValueObjects.Money.Create(0));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for order {OrderId}", request.OrderId);
            return new RefundPaymentResult(false, null, "An error occurred while processing the refund", Domain.ValueObjects.Money.Create(0));
        }
    }
}