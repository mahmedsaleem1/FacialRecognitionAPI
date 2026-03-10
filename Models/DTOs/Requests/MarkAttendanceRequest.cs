using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

public class MarkAttendanceRequest
{
    [Required]
    public string Uuid { get; set; } = string.Empty;
}
