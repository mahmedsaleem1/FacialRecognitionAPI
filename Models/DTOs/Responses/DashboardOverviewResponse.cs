namespace FacialRecognitionAPI.Models.DTOs.Responses;

/// <summary>
/// Overall dashboard analytics overview.
/// </summary>
public class DashboardOverviewResponse
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TodayPresent { get; set; }
    public int TodayLate { get; set; }
    public int TodayAbsent { get; set; }
    public double TodayAttendancePercentage { get; set; }
    public double WeeklyAverageAttendance { get; set; }
    public double MonthlyAverageAttendance { get; set; }
    public List<DepartmentSummary> DepartmentBreakdown { get; set; } = new();
}

public class DepartmentSummary
{
    public string Department { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int PresentToday { get; set; }
    public double AttendancePercentage { get; set; }
}
