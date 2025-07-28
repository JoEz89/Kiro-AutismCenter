using MediatR;

namespace AutismCenter.Application.Features.Orders.Commands.Admin.ProcessRefundAdmin;

public record ProcessRefundAdminCommand(
    Guid OrderId,
    decimal? RefundAmount = null, // If null, refund full amount
    string? Reason = null
) : IRequest<ProcessRefundAdminResponse>;