using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Mark check-out for an employee who has already checked in today.
/// </summary>
public class CheckOutRequest
{
    /// <summary>
    /// Base64-encoded face image for identity verification at check-out.
    /// </summary>
    [Required]
    public string FaceImageBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Direct employee ID for 1:1 verification.
    /// </summary>
    public Guid? EmployeeId { get; set; }
}
