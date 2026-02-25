namespace FacialRecognitionAPI.Models.Entities;

/// <summary>
/// Represents a single attendance record for an employee.
/// </summary>
public class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Date of attendance (date-only, no time component).
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Clock-in time.
    /// </summary>
    public TimeOnly CheckInTime { get; set; }

    /// <summary>
    /// Clock-out time (null if not yet clocked out).
    /// </summary>
    public TimeOnly? CheckOutTime { get; set; }

    /// <summary>
    /// Face similarity score at check-in (0.0 - 1.0).
    /// </summary>
    public float CheckInSimilarityScore { get; set; }

    /// <summary>
    /// Face similarity score at check-out.
    /// </summary>
    public float? CheckOutSimilarityScore { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    /// <summary>
    /// Optional admin note or auto-generated remark.
    /// </summary>
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Employee Employee { get; set; } = null!;
}

public enum AttendanceStatus
{
    Present = 0,
    Late = 1,
    HalfDay = 2,
    Absent = 3,
    Excused = 4
}
