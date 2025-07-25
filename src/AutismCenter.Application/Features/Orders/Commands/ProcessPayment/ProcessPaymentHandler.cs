using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Application.Features.Orders.Commands.ProcessPayment;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessPaymentHandler> _logger;

    public ProcessPaymentHandler(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        IUnitOfWork unitOfWork,
        ILogger<ProcessPaymentHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProcessPaymentResult> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new ProcessPaymentResult(false, null, "Order not found");
            }

            if (order.PaymentStatus == Domain.Enums.PaymentStatus.Completed)
            {
                return new ProcessPaymentResult(false, order.PaymentId, "Payment already completed");
            }

            var paymentRequest = new PaymentRequest(
                PaymentMethodId: request.PaymentMethodId,
                Amount: order.TotalAmount,
                Currency: order.TotalAmount.Currency.ToLowerInvariant(),
                Description: $"Payment for order {order.OrderNumber}",
                Metadata: new Dictionary<string, string>
                {
                    ["order_id"] = order.Id.ToString(),
                    ["order_number"] = order.OrderNumber,
                    ["user_id"] = order.UserId.ToString()
                });

            var paymentResult = await _paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);

            if (paymentResult.IsSuccess && !string.IsNullOrEmpty(paymentResult.PaymentId))
            {
                order.MarkPaymentCompleted(paymentResult.PaymentId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment processed successfully for order {OrderId}, payment ID: {PaymentId}", 
                    order.Id, paymentResult.PaymentId);

                return new ProcessPaymentResult(true, paymentResult.PaymentId, null);
            }
            else
            {
                order.MarkPaymentFailed();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Payment failed for order {OrderId}: {ErrorMessage}", 
                    order.Id, paymentResult.ErrorMessage);

                return new ProcessPaymentResult(false, null, paymentResult.ErrorMessage ?? "Payment processing failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
            return new ProcessPaymentResult(false, null, "An error occurred while processing the payment");
        }
    }
}