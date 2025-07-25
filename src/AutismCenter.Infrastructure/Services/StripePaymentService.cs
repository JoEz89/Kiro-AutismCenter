using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Common.Settings;
using AutismCenter.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace AutismCenter.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly RefundService _refundService;

    public StripePaymentService(
        IOptions<StripeSettings> stripeSettings,
        ILogger<StripePaymentService> logger)
    {
        _stripeSettings = stripeSettings.Value;
        _logger = logger;

        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        
        _paymentIntentService = new PaymentIntentService();
        _refundService = new RefundService();
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = ConvertToStripeAmount(request.Amount),
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethodId,
                Description = request.Description,
                ConfirmationMethod = "manual",
                Confirm = true,
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            var paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);

            return paymentIntent.Status switch
            {
                "succeeded" => new PaymentResult(true, paymentIntent.Id, null, Application.Common.Models.PaymentStatus.Succeeded),
                "requires_action" => new PaymentResult(false, paymentIntent.Id, "Payment requires additional action", Application.Common.Models.PaymentStatus.RequiresAction),
                "requires_payment_method" => new PaymentResult(false, paymentIntent.Id, "Payment method failed", Application.Common.Models.PaymentStatus.RequiresPaymentMethod),
                _ => new PaymentResult(false, paymentIntent.Id, $"Payment failed with status: {paymentIntent.Status}", Application.Common.Models.PaymentStatus.Failed)
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error processing payment: {ErrorMessage}", ex.Message);
            return new PaymentResult(false, null, ex.Message, Application.Common.Models.PaymentStatus.Failed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment");
            return new PaymentResult(false, null, "An unexpected error occurred", Application.Common.Models.PaymentStatus.Failed);
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(string paymentId, Money amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentId,
                Amount = ConvertToStripeAmount(amount),
                Reason = RefundReasons.RequestedByCustomer
            };

            var refund = await _refundService.CreateAsync(options, cancellationToken: cancellationToken);

            if (refund.Status == "succeeded")
            {
                var refundedAmount = Money.Create(ConvertFromStripeAmount(refund.Amount ?? 0), amount.Currency);
                return new RefundResult(true, refund.Id, null, refundedAmount);
            }
            else
            {
                return new RefundResult(false, refund.Id, $"Refund failed with status: {refund.Status}", Money.Create(0));
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error processing refund: {ErrorMessage}", ex.Message);
            return new RefundResult(false, null, ex.Message, Money.Create(0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing refund");
            return new RefundResult(false, null, "An unexpected error occurred", Money.Create(0));
        }
    }

    public async Task<Application.Common.Models.PaymentStatus> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentIntent = await _paymentIntentService.GetAsync(paymentId, cancellationToken: cancellationToken);

            return paymentIntent.Status switch
            {
                "succeeded" => Application.Common.Models.PaymentStatus.Succeeded,
                "requires_action" => Application.Common.Models.PaymentStatus.RequiresAction,
                "requires_payment_method" => Application.Common.Models.PaymentStatus.RequiresPaymentMethod,
                "canceled" => Application.Common.Models.PaymentStatus.Canceled,
                "processing" => Application.Common.Models.PaymentStatus.Pending,
                _ => Application.Common.Models.PaymentStatus.Failed
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting payment status: {ErrorMessage}", ex.Message);
            return Application.Common.Models.PaymentStatus.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting payment status");
            return Application.Common.Models.PaymentStatus.Failed;
        }
    }

    public async Task<string> CreatePaymentIntentAsync(Money amount, string currency, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = ConvertToStripeAmount(amount),
                Currency = currency,
                Metadata = metadata ?? new Dictionary<string, string>(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);
            return paymentIntent.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating payment intent: {ErrorMessage}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating payment intent");
            throw;
        }
    }

    public async Task<bool> ConfirmPaymentIntentAsync(string paymentIntentId, string paymentMethodId, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId
            };

            var paymentIntent = await _paymentIntentService.ConfirmAsync(paymentIntentId, options, cancellationToken: cancellationToken);
            return paymentIntent.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming payment intent: {ErrorMessage}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error confirming payment intent");
            return false;
        }
    }

    private static long ConvertToStripeAmount(Money amount)
    {
        // Stripe expects amounts in the smallest currency unit (e.g., cents for USD, fils for BHD)
        // BHD has 3 decimal places (1 BHD = 1000 fils)
        return amount.Currency.ToUpperInvariant() switch
        {
            "BHD" => (long)(amount.Amount * 1000),
            "USD" => (long)(amount.Amount * 100),
            "EUR" => (long)(amount.Amount * 100),
            _ => (long)(amount.Amount * 100) // Default to 2 decimal places
        };
    }

    private static decimal ConvertFromStripeAmount(long stripeAmount, string currency = "BHD")
    {
        return currency.ToUpperInvariant() switch
        {
            "BHD" => stripeAmount / 1000m,
            "USD" => stripeAmount / 100m,
            "EUR" => stripeAmount / 100m,
            _ => stripeAmount / 100m // Default to 2 decimal places
        };
    }
}