using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.GetOrdersAdmin;

public class GetOrdersAdminHandler : IRequestHandler<GetOrdersAdminQuery, GetOrdersAdminResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public GetOrdersAdminHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<GetOrdersAdminResponse> Handle(GetOrdersAdminQuery request, CancellationToken cancellationToken)
    {
        // Get all orders first (in a real implementation, this should be optimized with database-level filtering)
        var allOrders = await _orderRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filteredOrders = allOrders.AsQueryable();

        if (request.StartDate.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.CreatedAt <= request.EndDate.Value);
        }

        if (request.Status.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.Status == request.Status.Value);
        }

        if (request.PaymentStatus.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.PaymentStatus == request.PaymentStatus.Value);
        }

        // Apply search term (this would be more efficient with database-level search)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLowerInvariant();
            filteredOrders = filteredOrders.Where(o => 
                o.OrderNumber.ToLowerInvariant().Contains(searchTerm));
        }

        // Apply sorting
        filteredOrders = request.SortBy?.ToLowerInvariant() switch
        {
            "ordernumber" => request.SortDescending 
                ? filteredOrders.OrderByDescending(o => o.OrderNumber)
                : filteredOrders.OrderBy(o => o.OrderNumber),
            "status" => request.SortDescending 
                ? filteredOrders.OrderByDescending(o => o.Status)
                : filteredOrders.OrderBy(o => o.Status),
            "totalamount" => request.SortDescending 
                ? filteredOrders.OrderByDescending(o => o.TotalAmount.Amount)
                : filteredOrders.OrderBy(o => o.TotalAmount.Amount),
            _ => request.SortDescending 
                ? filteredOrders.OrderByDescending(o => o.CreatedAt)
                : filteredOrders.OrderBy(o => o.CreatedAt)
        };

        var totalCount = filteredOrders.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        // Apply pagination
        var pagedOrders = filteredOrders
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Convert to DTOs with customer information
        var orderDtos = new List<OrderAdminDto>();
        foreach (var order in pagedOrders)
        {
            var user = await _userRepository.GetByIdAsync(order.UserId, cancellationToken);
            var customerName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
            var customerEmail = user?.Email.Value ?? "Unknown";

            orderDtos.Add(new OrderAdminDto(
                order.Id,
                order.OrderNumber,
                customerName,
                customerEmail,
                order.CreatedAt,
                order.Status,
                order.PaymentStatus,
                order.TotalAmount.Amount,
                order.TotalAmount.Currency,
                order.Items.Count,
                order.ShippedAt,
                order.DeliveredAt,
                order.UpdatedAt));
        }

        return new GetOrdersAdminResponse(
            orderDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);
    }
}