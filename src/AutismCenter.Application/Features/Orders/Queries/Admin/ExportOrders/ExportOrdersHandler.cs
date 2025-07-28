using MediatR;
using AutismCenter.Domain.Interfaces;
using System.Text;
using System.Globalization;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.ExportOrders;

public class ExportOrdersHandler : IRequestHandler<ExportOrdersQuery, ExportOrdersResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public ExportOrdersHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<ExportOrdersResponse> Handle(ExportOrdersQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get orders within date range
        var orders = await _orderRepository.GetOrdersWithItemsByDateRangeAsync(startDate, endDate, cancellationToken);

        // Apply status filter
        if (request.Status.HasValue)
        {
            orders = orders.Where(o => o.Status == request.Status.Value);
        }

        var orderList = orders.OrderByDescending(o => o.CreatedAt).ToList();

        // Generate export based on format
        byte[] fileContent;
        string fileName;
        string contentType;

        switch (request.Format.ToUpperInvariant())
        {
            case "CSV":
                fileContent = await GenerateCsvExport(orderList, cancellationToken);
                fileName = $"orders_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                contentType = "text/csv";
                break;
            default:
                throw new ArgumentException($"Unsupported export format: {request.Format}");
        }

        return new ExportOrdersResponse(
            fileContent,
            fileName,
            contentType,
            orderList.Count);
    }

    private async Task<byte[]> GenerateCsvExport(IList<Domain.Entities.Order> orders, CancellationToken cancellationToken)
    {
        var csv = new StringBuilder();
        
        // CSV Header
        csv.AppendLine("Order Number,Customer Name,Customer Email,Order Date,Status,Payment Status,Total Amount,Currency,Items Count,Shipping Address,Billing Address,Shipped Date,Delivered Date");

        foreach (var order in orders)
        {
            var user = await _userRepository.GetByIdAsync(order.UserId, cancellationToken);
            var customerName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
            var customerEmail = user?.Email.Value ?? "Unknown";

            // Escape CSV values
            var shippingAddress = EscapeCsvValue($"{order.ShippingAddress.Street}, {order.ShippingAddress.City}, {order.ShippingAddress.Country}");
            var billingAddress = EscapeCsvValue($"{order.BillingAddress.Street}, {order.BillingAddress.City}, {order.BillingAddress.Country}");

            csv.AppendLine($"{EscapeCsvValue(order.OrderNumber)}," +
                          $"{EscapeCsvValue(customerName)}," +
                          $"{EscapeCsvValue(customerEmail)}," +
                          $"{order.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"{order.Status}," +
                          $"{order.PaymentStatus}," +
                          $"{order.TotalAmount.Amount.ToString(CultureInfo.InvariantCulture)}," +
                          $"{order.TotalAmount.Currency}," +
                          $"{order.Items.Count}," +
                          $"{shippingAddress}," +
                          $"{billingAddress}," +
                          $"{(order.ShippedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "")}," +
                          $"{(order.DeliveredAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "")}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // If the value contains comma, newline, or quote, wrap it in quotes and escape internal quotes
        if (value.Contains(',') || value.Contains('\n') || value.Contains('\r') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}