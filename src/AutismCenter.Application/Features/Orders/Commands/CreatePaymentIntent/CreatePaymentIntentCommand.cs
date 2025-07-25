using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.CreatePaymentIntent;

public record CreatePaymentIntentCommand(Guid OrderId) : IRequest<CreatePaymentIntentResult>;

public record CreatePaymentIntentResult(
    bool IsSuccess,
    string? ClientSecret,
    string? PaymentIntentId,
    string? ErrorMessage);