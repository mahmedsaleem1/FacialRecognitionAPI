namespace FacialRecognitionAPI.Models.Entities;

public class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "present";

    // Navigation
    public Employee Employee { get; set; } = null!;
}
