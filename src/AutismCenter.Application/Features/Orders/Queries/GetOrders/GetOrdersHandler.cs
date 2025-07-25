using AutismCenter.Application.Features.Orders.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, GetOrdersResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<GetOrdersResponse> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Order> orders;

        // Apply filters based on query parameters
        if (request.UserId.HasValue)
        {
            orders = await _orderRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        }
        else if (request.Status.HasValue)
        {
            orders = await _orderRepository.GetByStatusAsync(request.Status.Value, cancellationToken);
        }
        else if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            orders = await _orderRepository.GetByDateRangeAsync(request.StartDate.Value, request.EndDate.Value, cancellationToken);
        }
        else
        {
            orders = await _orderRepository.GetAllAsync(cancellationToken);
        }

        // Apply additional filters if needed
        if (request.Status.HasValue && request.UserId.HasValue)
        {
            orders = orders.Where(o => o.Status == request.Status.Value);
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue && (request.UserId.HasValue || request.Status.HasValue))
        {
            orders = orders.Where(o => o.CreatedAt >= request.StartDate.Value && o.CreatedAt <= request.EndDate.Value);
        }

        // Order by creation date (most recent first)
        orders = orders.OrderByDescending(o => o.CreatedAt);

        var totalCount = orders.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        // Apply pagination
        var paginatedOrders = orders
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var orderDtos = paginatedOrders.Select(OrderDto.FromEntity).ToList();

        return new GetOrdersResponse(
            orderDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages
        );
    }
}