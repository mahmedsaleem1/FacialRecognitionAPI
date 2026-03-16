using System.ComponentModel.DataAnnotations.Schema;

namespace FacialRecognitionAPI.Models.Entities;

public class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public int AttendanceStatusId { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public string Status
    {
        get => AttendanceStatus?.Name ?? "present";
        set { }
    }

    // Navigation
    public Employee Employee { get; set; } = null!;
    public AttendanceStatus AttendanceStatus { get; set; } = null!;
}
