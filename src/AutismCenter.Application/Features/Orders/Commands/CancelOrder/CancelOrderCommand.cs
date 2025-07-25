using AutismCenter.Application.Features.Orders.Common;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.CancelOrder;

public record CancelOrderCommand(
    Guid OrderId,
    Guid? UserId = null // Optional - for user-initiated cancellations
) : IRequest<OrderDto>;