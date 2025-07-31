using MediatR;

namespace AutismCenter.Application.Features.Dashboard.Queries.GetDashboardOverview;

public record GetDashboardOverviewQuery(
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<GetDashboardOverviewResponse>;