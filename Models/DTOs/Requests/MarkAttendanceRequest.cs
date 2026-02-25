using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Mark attendance by providing a face image. The system identifies the employee
/// by comparing against all registered face embeddings.
/// </summary>
public class MarkAttendanceRequest
{
    /// <summary>
    /// Base64-encoded face image captured at the attendance kiosk/device.
    /// </summary>
    [Required]
    public string FaceImageBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Optional: If the employee is already identified (e.g., by scanning an ID),
    /// provide their ID for a direct 1:1 verification instead of 1:N search.
    /// </summary>
    public Guid? EmployeeId { get; set; }
}
