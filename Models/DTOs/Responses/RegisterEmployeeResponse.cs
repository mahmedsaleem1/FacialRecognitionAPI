namespace FacialRecognitionAPI.Models.DTOs.Responses;

public class RegisterEmployeeResponse
{
    public string Uuid { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string JoinDate { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
