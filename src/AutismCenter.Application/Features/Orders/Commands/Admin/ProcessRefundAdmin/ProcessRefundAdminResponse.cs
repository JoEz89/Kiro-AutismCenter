using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Commands.Admin.ProcessRefundAdmin;

public record ProcessRefundAdminResponse(
    Guid OrderId,
    string OrderNumber,
    decimal RefundAmount,
    string Currency,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    DateTime ProcessedAt
);