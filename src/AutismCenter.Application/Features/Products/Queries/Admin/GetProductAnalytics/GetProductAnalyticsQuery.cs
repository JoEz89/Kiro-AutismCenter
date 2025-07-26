using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.Admin.GetProductAnalytics;

public record GetProductAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Guid? CategoryId = null
) : IRequest<GetProductAnalyticsResponse>;