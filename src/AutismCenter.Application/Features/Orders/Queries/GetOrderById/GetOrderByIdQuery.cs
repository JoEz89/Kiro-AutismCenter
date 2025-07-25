using AutismCenter.Application.Features.Orders.Common;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(
    Guid OrderId,
    Guid? UserId = null // Optional - for user-specific access control
) : IRequest<OrderDto?>;