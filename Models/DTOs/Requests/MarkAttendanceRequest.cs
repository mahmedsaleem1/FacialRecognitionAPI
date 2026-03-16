using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

public class MarkAttendanceRequest
{
    [Required]
    public string Uuid { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90, ErrorMessage = "latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "longitude must be between -180 and 180.")]
    public double Longitude { get; set; }
}
