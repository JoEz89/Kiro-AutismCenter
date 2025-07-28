using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.GetOrdersAdmin;

public record GetOrdersAdminResponse(
    IEnumerable<OrderAdminDto> Orders,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public record OrderAdminDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    DateTime OrderDate,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    decimal TotalAmount,
    string Currency,
    int ItemsCount,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime UpdatedAt
);