using AutismCenter.Application.Common.Models;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<RefundResult> ProcessRefundAsync(string paymentId, Money amount, CancellationToken cancellationToken = default);
    Task<PaymentStatus> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken = default);
    Task<string> CreatePaymentIntentAsync(Money amount, string currency, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    Task<bool> ConfirmPaymentIntentAsync(string paymentIntentId, string paymentMethodId, CancellationToken cancellationToken = default);
}

public record PaymentRequest(
    string PaymentMethodId,
    Money Amount,
    string Currency,
    string Description,
    Dictionary<string, string>? Metadata = null);

public record PaymentResult(
    bool IsSuccess,
    string? PaymentId,
    string? ErrorMessage,
    PaymentStatus Status);

public record RefundResult(
    bool IsSuccess,
    string? RefundId,
    string? ErrorMessage,
    Money RefundedAmount);

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Canceled,
    RequiresAction,
    RequiresPaymentMethod
}