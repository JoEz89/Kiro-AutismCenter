using AutismCenter.Domain.Enums;

namespace AutismCenter.WebApi.Models.Admin;

/// <summary>
/// Request model for getting orders with admin filtering
/// </summary>
public class GetOrdersAdminRequest
{
    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination (default: 20)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filter by order status
    /// </summary>
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Filter by payment status
    /// </summary>
    public PaymentStatus? PaymentStatus { get; set; }

    /// <summary>
    /// Start date for filtering orders
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering orders
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Search term for filtering by order number or customer email
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sort field (default: CreatedAt)
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort direction (asc/desc, default: desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Request model for getting order analytics
/// </summary>
public class GetOrderAnalyticsRequest
{
    /// <summary>
    /// Start date for analytics period
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for analytics period
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Group analytics by (day, week, month)
    /// </summary>
    public string GroupBy { get; set; } = "day";

    /// <summary>
    /// Filter by order status
    /// </summary>
    public OrderStatus? Status { get; set; }
}

/// <summary>
/// Request model for exporting orders
/// </summary>
public class ExportOrdersRequest
{
    /// <summary>
    /// Start date for export period
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for export period
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by order status
    /// </summary>
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Filter by payment status
    /// </summary>
    public PaymentStatus? PaymentStatus { get; set; }

    /// <summary>
    /// Export format (csv, excel)
    /// </summary>
    public string Format { get; set; } = "csv";
}

/// <summary>
/// Request model for updating order status
/// </summary>
public class UpdateOrderStatusAdminRequest
{
    /// <summary>
    /// New order status
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Optional notes for the status update
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for processing refunds
/// </summary>
public class ProcessRefundAdminRequest
{
    /// <summary>
    /// Refund amount (if partial refund)
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Reason for the refund
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Type of refund (full, partial)
    /// </summary>
    public string RefundType { get; set; } = "full";
}