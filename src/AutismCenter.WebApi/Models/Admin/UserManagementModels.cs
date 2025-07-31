using AutismCenter.Domain.Enums;

namespace AutismCenter.WebApi.Models.Admin;

/// <summary>
/// Request model for getting users with admin filtering
/// </summary>
public class GetUsersAdminRequest
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
    /// Search term for filtering users by name or email
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by user role
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

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
/// Request model for getting user analytics
/// </summary>
public class GetUserAnalyticsRequest
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
}

/// <summary>
/// Request model for updating user role
/// </summary>
public class UpdateUserRoleRequest
{
    /// <summary>
    /// New role for the user
    /// </summary>
    public UserRole Role { get; set; }
}