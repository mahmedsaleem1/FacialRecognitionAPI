namespace FacialRecognitionAPI.Models.DTOs.Responses.Dashboard;

public class AttendanceOverviewResponse
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public int TotalWorkingDays { get; set; }
    public int TotalEmployees { get; set; }
    public double AverageAttendanceRate { get; set; }
    public List<DailyAttendanceSummary> DailySummaries { get; set; } = [];
}

public class DailyAttendanceSummary
{
    public string Date { get; set; } = string.Empty;
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public double AttendanceRate { get; set; }
}

public class MonthlyAttendanceResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalWorkingDays { get; set; }
    public int TotalEmployees { get; set; }
    public double AverageAttendanceRate { get; set; }
    public List<EmployeeMonthlyRecord> Records { get; set; } = [];
}

public class EmployeeMonthlyRecord
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public double AttendanceRate { get; set; }
}

public class EmployeeAttendanceHistoryResponse
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int TotalDaysPresent { get; set; }
    public double AttendanceRate { get; set; }
    public List<AttendanceHistoryItem> History { get; set; } = [];
}

public class AttendanceHistoryItem
{
    public string Date { get; set; } = string.Empty;
    public string MarkedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
