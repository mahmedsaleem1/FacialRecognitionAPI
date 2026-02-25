namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Query parameters for analytics endpoints.
/// </summary>
public class AttendanceQueryParams
{
    /// <summary>
    /// Start date (inclusive). Defaults to first day of current month.
    /// </summary>
    public DateOnly? From { get; set; }

    /// <summary>
    /// End date (inclusive). Defaults to today.
    /// </summary>
    public DateOnly? To { get; set; }

    /// <summary>
    /// Filter by department.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Filter by specific employee ID.
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Page number (1-based). Default = 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size. Default = 20. Max = 100.
    /// </summary>
    public int PageSize { get; set; } = 20;
}
