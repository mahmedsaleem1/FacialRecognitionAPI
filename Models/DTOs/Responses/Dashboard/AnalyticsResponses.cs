namespace FacialRecognitionAPI.Models.DTOs.Responses.Dashboard;

public class AttendanceTrendResponse
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public List<TrendDataPoint> DataPoints { get; set; } = [];
}

public class TrendDataPoint
{
    public string Date { get; set; } = string.Empty;
    public int PresentCount { get; set; }
    public int TotalEmployees { get; set; }
    public double AttendanceRate { get; set; }
}

public class DepartmentStatsResponse
{
    public List<DepartmentStat> Departments { get; set; } = [];
}

public class DepartmentStat
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public double AverageAttendanceRate { get; set; }
    public int TodayPresentCount { get; set; }
    public int TodayAbsentCount { get; set; }
}

public class TopAttendeeResponse
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public List<AttendeeRanking> Rankings { get; set; } = [];
}

public class AttendeeRanking
{
    public int Rank { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int DaysPresent { get; set; }
    public int TotalWorkingDays { get; set; }
    public double AttendanceRate { get; set; }
}

public class LowAttendeeResponse
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public List<AttendeeRanking> Employees { get; set; } = [];
}
