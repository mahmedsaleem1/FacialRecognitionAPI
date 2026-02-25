namespace FacialRecognitionAPI.Models.DTOs.Responses;

public class EmployeeResponse
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string CloudinaryImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
