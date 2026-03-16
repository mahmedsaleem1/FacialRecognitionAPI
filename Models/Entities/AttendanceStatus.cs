namespace FacialRecognitionAPI.Models.Entities;

public class AttendanceStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
