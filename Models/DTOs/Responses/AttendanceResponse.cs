using FacialRecognitionAPI.Models.Entities;

namespace FacialRecognitionAPI.Models.DTOs.Responses;

public class AttendanceResponse
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public float CheckInSimilarityScore { get; set; }
    public float? CheckOutSimilarityScore { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
}
