using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Multipart/form-data version of the onboard request.
/// Send the image as a file upload — no Base64 conversion needed.
/// </summary>
public class OnboardEmployeeFormRequest
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    /// <summary>
    /// The employee's face photo (JPEG or PNG).
    /// </summary>
    [Required]
    public IFormFile FaceImage { get; set; } = null!;

    /// <summary>
    /// Date the employee joined (defaults to today if omitted).
    /// </summary>
    public DateTime? JoinDate { get; set; }
}
