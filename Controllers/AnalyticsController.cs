using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FacialRecognitionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get the admin dashboard overview.
    /// Includes total employees, today's attendance summary, weekly/monthly averages,
    /// and per-department breakdown.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardOverviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var overview = await _analyticsService.GetDashboardOverviewAsync(cancellationToken);
        return Ok(ApiResponse<DashboardOverviewResponse>.Ok(overview));
    }

    /// <summary>
    /// Get attendance summary for a specific date.
    /// </summary>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(ApiResponse<DailyAttendanceSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var summary = await _analyticsService.GetDailySummaryAsync(targetDate, cancellationToken);
        return Ok(ApiResponse<DailyAttendanceSummary>.Ok(summary));
    }

    /// <summary>
    /// Get attendance summaries for an entire week (Mon–Fri).
    /// Pass the Monday date or the API defaults to the current week.
    /// </summary>
    [HttpGet("weekly")]
    [ProducesResponseType(typeof(ApiResponse<List<DailyAttendanceSummary>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklySummary([FromQuery] DateOnly? weekStart, CancellationToken cancellationToken)
    {
        var monday = weekStart ?? GetCurrentMonday();
        var summaries = await _analyticsService.GetWeeklySummaryAsync(monday, cancellationToken);
        return Ok(ApiResponse<List<DailyAttendanceSummary>>.Ok(summaries));
    }

    /// <summary>
    /// Get monthly attendance reports for all employees (or filtered by department).
    /// </summary>
    [HttpGet("monthly")]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeMonthlyReport>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyReports(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string? department,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        var reports = await _analyticsService.GetMonthlyReportsAsync(targetYear, targetMonth, department, cancellationToken);
        return Ok(ApiResponse<List<EmployeeMonthlyReport>>.Ok(reports));
    }

    /// <summary>
    /// Get a specific employee's monthly attendance report.
    /// </summary>
    [HttpGet("employee/{employeeId:guid}/monthly")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeMonthlyReport>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeMonthlyReport(
        Guid employeeId,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        var report = await _analyticsService.GetEmployeeMonthlyReportAsync(employeeId, targetYear, targetMonth, cancellationToken);
        return Ok(ApiResponse<EmployeeMonthlyReport>.Ok(report));
    }

    private static DateOnly GetCurrentMonday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-daysFromMonday);
    }
}
