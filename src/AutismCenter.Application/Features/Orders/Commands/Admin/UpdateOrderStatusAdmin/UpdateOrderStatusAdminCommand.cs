using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Commands.Admin.UpdateOrderStatusAdmin;

public record UpdateOrderStatusAdminCommand(
    Guid OrderId,
    OrderStatus Status,
    string? AdminNotes = null
) : IRequest<UpdateOrderStatusAdminResponse>;