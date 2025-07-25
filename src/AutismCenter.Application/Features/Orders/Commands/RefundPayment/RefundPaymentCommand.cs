using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.RefundPayment;

public record RefundPaymentCommand(
    Guid OrderId,
    Money? RefundAmount = null,
    string? Reason = null) : IRequest<RefundPaymentResult>;

public record RefundPaymentResult(
    bool IsSuccess,
    string? RefundId,
    string? ErrorMessage,
    Money RefundedAmount);