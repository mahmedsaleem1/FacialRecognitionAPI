using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Replace an employee's reference face image.
/// </summary>
public class UpdateEmployeeFaceRequest
{
    /// <summary>
    /// Base64-encoded new reference face image (JPEG/PNG).
    /// </summary>
    [Required]
    public string FaceImageBase64 { get; set; } = string.Empty;
}
