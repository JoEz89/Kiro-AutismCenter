using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Commands.Admin.UpdateOrderStatusAdmin;

public record UpdateOrderStatusAdminResponse(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    DateTime UpdatedAt
);