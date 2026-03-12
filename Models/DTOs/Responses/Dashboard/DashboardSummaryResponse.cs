namespace FacialRecognitionAPI.Models.DTOs.Responses.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalEmployees { get; set; }
    public int TodayPresentCount { get; set; }
    public int TodayAbsentCount { get; set; }
    public double TodayAttendanceRate { get; set; }
    public int NewEmployeesThisMonth { get; set; }
    public double AverageAttendanceRateLast30Days { get; set; }
    public List<RecentActivityItem> RecentActivity { get; set; } = [];
}

public class RecentActivityItem
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string MarkedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
