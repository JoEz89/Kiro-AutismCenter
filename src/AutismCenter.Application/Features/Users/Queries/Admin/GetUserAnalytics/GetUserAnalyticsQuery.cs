using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Users.Queries.Admin.GetUserAnalytics;

public record GetUserAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    UserRole? Role = null
) : IRequest<GetUserAnalyticsResponse>;