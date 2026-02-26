using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Multipart/form-data version of the face update request.
/// </summary>
public class UpdateEmployeeFaceFormRequest
{
    /// <summary>
    /// The new reference face photo (JPEG or PNG).
    /// </summary>
    [Required]
    public IFormFile FaceImage { get; set; } = null!;
}
