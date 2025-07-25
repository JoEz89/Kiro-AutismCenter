using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutismCenter.Application.Features.Orders.Commands.CreatePaymentIntent;

public class CreatePaymentIntentHandler : IRequestHandler<CreatePaymentIntentCommand, CreatePaymentIntentResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CreatePaymentIntentHandler> _logger;

    public CreatePaymentIntentHandler(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        ILogger<CreatePaymentIntentHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<CreatePaymentIntentResult> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return new CreatePaymentIntentResult(false, null, null, "Order not found");
            }

            if (order.PaymentStatus == Domain.Enums.PaymentStatus.Completed)
            {
                return new CreatePaymentIntentResult(false, null, null, "Order has already been paid");
            }

            var metadata = new Dictionary<string, string>
            {
                ["order_id"] = order.Id.ToString(),
                ["order_number"] = order.OrderNumber,
                ["user_id"] = order.UserId.ToString()
            };

            var paymentIntentId = await _paymentService.CreatePaymentIntentAsync(
                order.TotalAmount,
                order.TotalAmount.Currency.ToLowerInvariant(),
                metadata,
                cancellationToken);

            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                _logger.LogInformation("Payment intent created successfully for order {OrderId}: {PaymentIntentId}", 
                    order.Id, paymentIntentId);

                // The client secret is typically the payment intent ID with a suffix
                var clientSecret = $"{paymentIntentId}_secret";
                
                return new CreatePaymentIntentResult(true, clientSecret, paymentIntentId, null);
            }
            else
            {
                _logger.LogWarning("Failed to create payment intent for order {OrderId}", order.Id);
                return new CreatePaymentIntentResult(false, null, null, "Failed to create payment intent");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for order {OrderId}", request.OrderId);
            return new CreatePaymentIntentResult(false, null, null, "An error occurred while creating payment intent");
        }
    }
}