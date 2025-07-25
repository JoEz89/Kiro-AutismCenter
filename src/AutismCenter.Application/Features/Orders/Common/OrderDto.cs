using AutismCenter.Application.Features.Orders.Commands.CreateOrder;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Common;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    decimal TotalAmount,
    string Currency,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    string? PaymentId,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    List<OrderItemDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt
)
{
    public static OrderDto FromEntity(Order order)
    {
        return new OrderDto(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.Status,
            order.PaymentStatus,
            order.PaymentId,
            new AddressDto(
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Country
            ),
            new AddressDto(
                order.BillingAddress.Street,
                order.BillingAddress.City,
                order.BillingAddress.State,
                order.BillingAddress.PostalCode,
                order.BillingAddress.Country
            ),
            order.Items.Select(OrderItemDto.FromEntity).ToList(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ShippedAt,
            order.DeliveredAt
        );
    }
}

public record OrderItemDto(
    Guid Id,
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    decimal TotalPrice
)
{
    public static OrderItemDto FromEntity(OrderItem orderItem)
    {
        return new OrderItemDto(
            orderItem.Id,
            orderItem.OrderId,
            orderItem.ProductId,
            orderItem.Product?.GetName(false) ?? "Unknown Product",
            orderItem.Quantity,
            orderItem.UnitPrice.Amount,
            orderItem.UnitPrice.Currency,
            orderItem.GetTotalPrice().Amount
        );
    }
}