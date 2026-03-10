namespace FacialRecognitionAPI.Models.DTOs.Responses;

public class DailyAttendanceRecord
{
    public string AttendanceId { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string MarkedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
