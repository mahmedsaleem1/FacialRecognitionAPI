using FacialRecognitionAPI.Models.DTOs.Responses.Dashboard;

namespace FacialRecognitionAPI.Services.Interfaces;

public interface IDashboardService
{
    // ── Summary ─────────────────────────────────────
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(CancellationToken ct = default);

    // ── Employees ───────────────────────────────────
    Task<EmployeeListResponse> GetEmployeesAsync(string? search, string? department, int page, int pageSize, CancellationToken ct = default);
    Task<EmployeeDetailResponse> GetEmployeeByIdAsync(Guid employeeId, CancellationToken ct = default);
    Task<List<DepartmentStat>> GetEmployeesByDepartmentAsync(CancellationToken ct = default);

    // ── Attendance ──────────────────────────────────
    Task DeleteAttendanceAsync(Guid attendanceId, CancellationToken ct = default);
    Task<AttendanceOverviewResponse> GetAttendanceOverviewAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<MonthlyAttendanceResponse> GetMonthlyAttendanceAsync(int year, int month, CancellationToken ct = default);
    Task<EmployeeAttendanceHistoryResponse> GetEmployeeAttendanceHistoryAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken ct = default);

    // ── Analytics ───────────────────────────────────
    Task<AttendanceTrendResponse> GetAttendanceTrendAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<DepartmentStatsResponse> GetDepartmentStatsAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<TopAttendeeResponse> GetTopAttendeesAsync(DateOnly from, DateOnly to, int count, CancellationToken ct = default);
    Task<LowAttendeeResponse> GetLowAttendeesAsync(DateOnly from, DateOnly to, double threshold, CancellationToken ct = default);
}
