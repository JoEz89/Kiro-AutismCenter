using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Queries.GetOrderHistory;

public class GetOrderHistoryHandler : IRequestHandler<GetOrderHistoryQuery, GetOrderHistoryResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHistoryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<GetOrderHistoryResponse> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        // Order by creation date (most recent first)
        var orderedList = orders.OrderByDescending(o => o.CreatedAt).ToList();

        var totalCount = orderedList.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        // Apply pagination
        var paginatedOrders = orderedList
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var orderDtos = paginatedOrders.Select(OrderDto.FromEntity).ToList();

        return new GetOrderHistoryResponse(
            orderDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages
        );
    }
}