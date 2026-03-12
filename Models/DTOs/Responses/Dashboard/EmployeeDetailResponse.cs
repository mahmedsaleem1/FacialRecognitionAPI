namespace FacialRecognitionAPI.Models.DTOs.Responses.Dashboard;

public class EmployeeDetailResponse
{
    public string Uuid { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string JoinDate { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public int TotalAttendanceDays { get; set; }
    public double AttendanceRate { get; set; }
    public string? LastAttendanceDate { get; set; }
}

public class EmployeeListResponse
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<EmployeeDetailResponse> Employees { get; set; } = [];
}
