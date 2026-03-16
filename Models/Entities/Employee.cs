using System.ComponentModel.DataAnnotations.Schema;

namespace FacialRecognitionAPI.Models.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public DateOnly JoinDate { get; set; }
    public string? FaceImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public string? Department
    {
        get => DepartmentLookup?.Name;
        set { }
    }

    [NotMapped]
    public string? Position
    {
        get => PositionLookup?.Name;
        set { }
    }

    // Navigation
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public Department? DepartmentLookup { get; set; }
    public JobPosition? PositionLookup { get; set; }
}
