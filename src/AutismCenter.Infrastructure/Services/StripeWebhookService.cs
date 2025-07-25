using AutismCenter.Application.Common.Settings;
using AutismCenter.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace AutismCenter.Infrastructure.Services;

public interface IStripeWebhookService
{
    Task<bool> HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
}

public class StripeWebhookService : IStripeWebhookService
{
    private readonly StripeSettings _stripeSettings;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StripeWebhookService> _logger;

    public StripeWebhookService(
        IOptions<StripeSettings> stripeSettings,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<StripeWebhookService> logger)
    {
        _stripeSettings = stripeSettings.Value;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _stripeSettings.WebhookSecret);

            _logger.LogInformation("Received Stripe webhook event: {EventType} with ID: {EventId}", 
                stripeEvent.Type, stripeEvent.Id);

            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    await HandlePaymentIntentSucceeded(stripeEvent, cancellationToken);
                    break;

                case Events.PaymentIntentPaymentFailed:
                    await HandlePaymentIntentFailed(stripeEvent, cancellationToken);
                    break;

                case Events.PaymentIntentCanceled:
                    await HandlePaymentIntentCanceled(stripeEvent, cancellationToken);
                    break;

                case Events.ChargeDisputeCreated:
                    await HandleChargeDispute(stripeEvent, cancellationToken);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe webhook event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed: {ErrorMessage}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return false;
        }
    }

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            _logger.LogWarning("PaymentIntent object is null in webhook event");
            return;
        }

        if (!paymentIntent.Metadata.TryGetValue("order_id", out var orderIdString) ||
            !Guid.TryParse(orderIdString, out var orderId))
        {
            _logger.LogWarning("Order ID not found in PaymentIntent metadata");
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        if (order.PaymentStatus != Domain.Enums.PaymentStatus.Completed)
        {
            order.MarkPaymentCompleted(paymentIntent.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment completed for order {OrderId} via webhook", orderId);
        }
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            _logger.LogWarning("PaymentIntent object is null in webhook event");
            return;
        }

        if (!paymentIntent.Metadata.TryGetValue("order_id", out var orderIdString) ||
            !Guid.TryParse(orderIdString, out var orderId))
        {
            _logger.LogWarning("Order ID not found in PaymentIntent metadata");
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        if (order.PaymentStatus != Domain.Enums.PaymentStatus.Failed)
        {
            order.MarkPaymentFailed();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment failed for order {OrderId} via webhook", orderId);
        }
    }

    private async Task HandlePaymentIntentCanceled(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            _logger.LogWarning("PaymentIntent object is null in webhook event");
            return;
        }

        if (!paymentIntent.Metadata.TryGetValue("order_id", out var orderIdString) ||
            !Guid.TryParse(orderIdString, out var orderId))
        {
            _logger.LogWarning("Order ID not found in PaymentIntent metadata");
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        if (order.CanBeCancelled())
        {
            order.Cancel();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} cancelled via webhook", orderId);
        }
    }

    private async Task HandleChargeDispute(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Dispute dispute)
        {
            _logger.LogWarning("Dispute object is null in webhook event");
            return;
        }

        _logger.LogWarning("Charge dispute created for charge: {ChargeId}, amount: {Amount}", 
            dispute.ChargeId, dispute.Amount);

        // Here you could implement additional logic to handle disputes
        // such as notifying administrators, updating order status, etc.
    }
}