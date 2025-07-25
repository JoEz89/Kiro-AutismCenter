using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Domain.Enums;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus
) : IRequest<OrderDto>;