using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid UserId,
    List<CreateOrderItemDto> Items,
    AddressDto ShippingAddress,
    AddressDto BillingAddress
) : IRequest<OrderDto>;

public record CreateOrderItemDto(
    Guid ProductId,
    int Quantity
);

public record AddressDto(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
)
{
    public Address ToValueObject()
    {
        return Address.Create(Street, City, State, PostalCode, Country);
    }
}