namespace FacialRecognitionAPI.Models.DTOs.Responses;

/// <summary>
/// Daily attendance summary for the admin dashboard.
/// </summary>
public class DailyAttendanceSummary
{
    public DateOnly Date { get; set; }
    public int TotalEmployees { get; set; }
    public int PresentCount { get; set; }
    public int LateCount { get; set; }
    public int HalfDayCount { get; set; }
    public int AbsentCount { get; set; }
    public int ExcusedCount { get; set; }
    public double AttendancePercentage { get; set; }
}
