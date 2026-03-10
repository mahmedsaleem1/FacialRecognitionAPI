namespace FacialRecognitionAPI.Models.DTOs.Responses;

public class MarkAttendanceResponse
{
    public string AttendanceId { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public string MarkedAt { get; set; } = string.Empty;
    public string Status { get; set; } = "present";
}
