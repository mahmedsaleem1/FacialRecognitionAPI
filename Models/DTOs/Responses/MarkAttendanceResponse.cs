namespace FacialRecognitionAPI.Models.DTOs.Responses;

/// <summary>
/// Result of marking attendance via face recognition.
/// </summary>
public class MarkAttendanceResponse
{
    public bool Recognized { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public float SimilarityScore { get; set; }
    public string? CheckInTime { get; set; }
    public string? Status { get; set; }
    public string Message { get; set; } = string.Empty;
}
