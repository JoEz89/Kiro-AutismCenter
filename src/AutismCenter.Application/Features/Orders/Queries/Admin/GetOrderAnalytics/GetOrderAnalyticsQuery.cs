using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.GetOrderAnalytics;

public record GetOrderAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    OrderStatus? Status = null,
    Guid? UserId = null
) : IRequest<GetOrderAnalyticsResponse>;