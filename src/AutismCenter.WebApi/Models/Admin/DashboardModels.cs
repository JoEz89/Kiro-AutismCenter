namespace AutismCenter.WebApi.Models.Admin;

/// <summary>
/// Request model for getting dashboard overview
/// </summary>
public class GetDashboardOverviewRequest
{
    /// <summary>
    /// Start date for analytics period
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for analytics period
    /// </summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Request model for getting system analytics
/// </summary>
public class GetSystemAnalyticsRequest
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
    /// Include user analytics in response
    /// </summary>
    public bool IncludeUsers { get; set; } = true;

    /// <summary>
    /// Include product analytics in response
    /// </summary>
    public bool IncludeProducts { get; set; } = true;

    /// <summary>
    /// Include order analytics in response
    /// </summary>
    public bool IncludeOrders { get; set; } = true;

    /// <summary>
    /// Include appointment analytics in response
    /// </summary>
    public bool IncludeAppointments { get; set; } = true;

    /// <summary>
    /// Include course analytics in response
    /// </summary>
    public bool IncludeCourses { get; set; } = true;
}