using System.ComponentModel.DataAnnotations;

namespace FacialRecognitionAPI.Models.DTOs.Requests;

/// <summary>
/// Update employee details (not the face image — use UpdateEmployeeFaceRequest for that).
/// </summary>
public class UpdateEmployeeRequest
{
    [MaxLength(200)]
    public string? FullName { get; set; }

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    public bool? IsActive { get; set; }
}
