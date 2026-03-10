using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

public class RegisterEmployeeFormRequest
{
    [Required, MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    [Required]
    public string JoinDate { get; set; } = string.Empty; // YYYY-MM-DD

    [Required]
    public IFormFile FaceImage { get; set; } = null!;
}
