using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;

namespace FacialRecognitionAPI.Services.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardOverviewResponse> GetDashboardOverviewAsync(CancellationToken cancellationToken = default);
    Task<DailyAttendanceSummary> GetDailySummaryAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<List<DailyAttendanceSummary>> GetWeeklySummaryAsync(DateOnly weekStart, CancellationToken cancellationToken = default);
    Task<List<EmployeeMonthlyReport>> GetMonthlyReportsAsync(int year, int month, string? department = null, CancellationToken cancellationToken = default);
    Task<EmployeeMonthlyReport> GetEmployeeMonthlyReportAsync(Guid employeeId, int year, int month, CancellationToken cancellationToken = default);
}
