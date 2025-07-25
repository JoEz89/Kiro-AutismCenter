using AutismCenter.Application.Features.Orders.Common;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Queries.GetOrderHistory;

public record GetOrderHistoryQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<GetOrderHistoryResponse>;

public record GetOrderHistoryResponse(
    List<OrderDto> Orders,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);