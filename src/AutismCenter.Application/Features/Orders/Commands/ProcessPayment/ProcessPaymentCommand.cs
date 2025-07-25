using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    Guid OrderId,
    string PaymentMethodId,
    string? PaymentIntentId = null) : IRequest<ProcessPaymentResult>;

public record ProcessPaymentResult(
    bool IsSuccess,
    string? PaymentId,
    string? ErrorMessage,
    string? ClientSecret = null);