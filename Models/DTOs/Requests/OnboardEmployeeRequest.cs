using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Onboard a new employee/internee. Image is uploaded as Base64.
/// </summary>
public class OnboardEmployeeRequest
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
    /// Base64-encoded reference face image (JPEG/PNG).
    /// </summary>
    [Required]
    public string FaceImageBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Date the employee/internee joined (defaults to today if omitted).
    /// </summary>
    public DateTime? JoinDate { get; set; }
}
