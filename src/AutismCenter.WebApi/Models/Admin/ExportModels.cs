namespace AutismCenter.WebApi.Models.Admin;

/// <summary>
/// Request model for exporting users
/// </summary>
public class ExportUsersRequest
{
    /// <summary>
    /// Filter by user role
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Start date for filtering users by creation date
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering users by creation date
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Export format (CSV, Excel)
    /// </summary>
    public string Format { get; set; } = "CSV";
}

/// <summary>
/// Request model for exporting products
/// </summary>
public class ExportProductsRequest
{
    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Include only low stock products
    /// </summary>
    public bool LowStockOnly { get; set; } = false;

    /// <summary>
    /// Export format (CSV, Excel)
    /// </summary>
    public string Format { get; set; } = "CSV";
}

/// <summary>
/// Request model for exporting appointments
/// </summary>
public class ExportAppointmentsRequest
{
    /// <summary>
    /// Start date for filtering appointments
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering appointments
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by appointment status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by doctor ID
    /// </summary>
    public Guid? DoctorId { get; set; }

    /// <summary>
    /// Export format (CSV, Excel)
    /// </summary>
    public string Format { get; set; } = "CSV";
}