namespace FacialRecognitionAPI.Models.DTOs.Responses;

/// <summary>
/// Monthly attendance report for an individual employee.
/// </summary>
public class EmployeeMonthlyReport
{
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int WorkingDays { get; set; }
    public int DaysPresent { get; set; }
    public int DaysLate { get; set; }
    public int DaysHalfDay { get; set; }
    public int DaysAbsent { get; set; }
    public int DaysExcused { get; set; }
    public double AttendancePercentage { get; set; }
    public string? AverageCheckInTime { get; set; }
    public string? AverageCheckOutTime { get; set; }
}
