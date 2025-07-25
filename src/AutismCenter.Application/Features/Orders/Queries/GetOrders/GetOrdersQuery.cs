using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Domain.Enums;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery(
    Guid? UserId = null,
    OrderStatus? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<GetOrdersResponse>;

public record GetOrdersResponse(
    List<OrderDto> Orders,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);