using MediatR;

namespace AutismCenter.Application.Features.Analytics.Queries.GetSystemAnalytics;

public record GetSystemAnalyticsQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    bool IncludeUsers,
    bool IncludeProducts,
    bool IncludeOrders,
    bool IncludeAppointments,
    bool IncludeCourses
) : IRequest<GetSystemAnalyticsResponse>;