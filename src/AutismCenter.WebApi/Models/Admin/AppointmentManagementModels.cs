using AutismCenter.Domain.Enums;

namespace AutismCenter.WebApi.Models.Admin;

/// <summary>
/// Request model for getting appointments with admin filtering
/// </summary>
public class GetAppointmentsAdminRequest
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
    /// Filter by appointment status
    /// </summary>
    public AppointmentStatus? Status { get; set; }

    /// <summary>
    /// Filter by doctor ID
    /// </summary>
    public Guid? DoctorId { get; set; }

    /// <summary>
    /// Start date for filtering appointments
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering appointments
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Search term for filtering by patient name, email, or doctor name
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sort field (default: AppointmentDate)
    /// </summary>
    public string SortBy { get; set; } = "AppointmentDate";

    /// <summary>
    /// Sort direction (asc/desc, default: desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Request model for getting appointment analytics
/// </summary>
public class GetAppointmentAnalyticsRequest
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
    /// Filter by doctor ID
    /// </summary>
    public Guid? DoctorId { get; set; }
}

/// <summary>
/// Request model for updating appointment status
/// </summary>
public class UpdateAppointmentStatusAdminRequest
{
    /// <summary>
    /// New appointment status
    /// </summary>
    public AppointmentStatus Status { get; set; }

    /// <summary>
    /// Optional notes for the status update
    /// </summary>
    public string? Notes { get; set; }
}