using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Multipart/form-data version of the check-out request.
/// </summary>
public class CheckOutFormRequest
{
    /// <summary>
    /// Face photo for check-out verification (JPEG or PNG).
    /// </summary>
    [Required]
    public IFormFile FaceImage { get; set; } = null!;

    /// <summary>
    /// Optional: Employee ID for 1:1 verification. Omit for 1:N identification.
    /// </summary>
    public Guid? EmployeeId { get; set; }
}
