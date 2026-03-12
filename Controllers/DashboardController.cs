using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FacialRecognitionAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    // ─────────────────────────────────────────────
    //  Summary
    // ─────────────────────────────────────────────

    /// <summary>
    /// Dashboard overview: totals, today's attendance, recent activity.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _dashboardService.GetDashboardSummaryAsync(ct);
        return Ok(result);
    }

    // ─────────────────────────────────────────────
    //  Employees
    // ─────────────────────────────────────────────

    /// <summary>
    /// Paginated employee list with search and department filter.
    /// </summary>
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var result = await _dashboardService.GetEmployeesAsync(search, department, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Single employee detail with attendance stats.
    /// </summary>
    [HttpGet("employees/{id:guid}")]
    public async Task<IActionResult> GetEmployeeById(Guid id, CancellationToken ct)
    {
        var result = await _dashboardService.GetEmployeeByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Employees grouped by department with today's attendance counts.
    /// </summary>
    [HttpGet("employees/by-department")]
    public async Task<IActionResult> GetEmployeesByDepartment(CancellationToken ct)
    {
        var result = await _dashboardService.GetEmployeesByDepartmentAsync(ct);
        return Ok(new { departments = result });
    }

    // ─────────────────────────────────────────────
    //  Attendance
    // ─────────────────────────────────────────────

    /// <summary>
    /// Delete an attendance record by its ID.
    /// </summary>
    [HttpDelete("attendance/{id:guid}")]
    public async Task<IActionResult> DeleteAttendance(Guid id, CancellationToken ct)
    {
        await _dashboardService.DeleteAttendanceAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Attendance overview for a date range: daily present/absent counts.
    /// Defaults to last 7 days.
    /// </summary>
    [HttpGet("attendance/overview")]
    public async Task<IActionResult> GetAttendanceOverview(
        [FromQuery] string? from,
        [FromQuery] string? to,
        CancellationToken ct = default)
    {
        var toDate = ParseDateOrDefault(to, DateOnly.FromDateTime(DateTime.UtcNow));
        var fromDate = ParseDateOrDefault(from, toDate.AddDays(-6));

        if (fromDate > toDate)
            return BadRequest(new { message = "'from' must be before or equal to 'to'." });

        var result = await _dashboardService.GetAttendanceOverviewAsync(fromDate, toDate, ct);
        return Ok(result);
    }

    /// <summary>
    /// Monthly attendance report: per-employee present/absent for a month.
    /// Defaults to current month.
    /// </summary>
    [HttpGet("attendance/monthly")]
    public async Task<IActionResult> GetMonthlyAttendance(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        if (m < 1 || m > 12)
            return BadRequest(new { message = "month must be between 1 and 12." });

        var result = await _dashboardService.GetMonthlyAttendanceAsync(y, m, ct);
        return Ok(result);
    }

    /// <summary>
    /// Attendance history for a specific employee in a date range.
    /// Defaults to last 30 days.
    /// </summary>
    [HttpGet("attendance/employee/{id:guid}")]
    public async Task<IActionResult> GetEmployeeAttendanceHistory(
        Guid id,
        [FromQuery] string? from,
        [FromQuery] string? to,
        CancellationToken ct = default)
    {
        var toDate = ParseDateOrDefault(to, DateOnly.FromDateTime(DateTime.UtcNow));
        var fromDate = ParseDateOrDefault(from, toDate.AddDays(-29));

        if (fromDate > toDate)
            return BadRequest(new { message = "'from' must be before or equal to 'to'." });

        var result = await _dashboardService.GetEmployeeAttendanceHistoryAsync(id, fromDate, toDate, ct);
        return Ok(result);
    }

    // ─────────────────────────────────────────────
    //  Analytics
    // ─────────────────────────────────────────────

    /// <summary>
    /// Attendance trend data points for charts. Defaults to last 30 days.
    /// </summary>
    [HttpGet("analytics/attendance-trend")]
    public async Task<IActionResult> GetAttendanceTrend(
        [FromQuery] string? from,
        [FromQuery] string? to,
        CancellationToken ct = default)
    {
        var toDate = ParseDateOrDefault(to, DateOnly.FromDateTime(DateTime.UtcNow));
        var fromDate = ParseDateOrDefault(from, toDate.AddDays(-29));

        if (fromDate > toDate)
            return BadRequest(new { message = "'from' must be before or equal to 'to'." });

        var result = await _dashboardService.GetAttendanceTrendAsync(fromDate, toDate, ct);
        return Ok(result);
    }

    /// <summary>
    /// Department-wise attendance analytics. Defaults to last 30 days.
    /// </summary>
    [HttpGet("analytics/department-stats")]
    public async Task<IActionResult> GetDepartmentStats(
        [FromQuery] string? from,
        [FromQuery] string? to,
        CancellationToken ct = default)
    {
        var toDate = ParseDateOrDefault(to, DateOnly.FromDateTime(DateTime.UtcNow));
        var fromDate = ParseDateOrDefault(from, toDate.AddDays(-29));

        if (fromDate > toDate)
            return BadRequest(new { message = "'from' must be before or equal to 'to'." });

        var result = await _dashboardService.GetDepartmentStatsAsync(fromDate, toDate, ct);
        return Ok(result);
    }

    /// <summary>
    /// Top attendees ranked by attendance rate. Defaults to last 30 days, top 10.
    /// </summary>
    [HttpGet("analytics/top-attendees")]
    public async Task<IActionResult> GetTopAttendees(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var toDate = ParseDateOrDefault(to, DateOnly.FromDateTime(DateTime.UtcNow));
        var fromDate = ParseDateOrDefault(from, toDate.AddDays(-29));

        if (fromDate > toDate)
            return BadRequest(new { message = "'from' must be before or equal to 'to'." });
        if (count < 1) count = 1;
        if (count > 100) count = 100;

        var result = await _dashboardService.GetTopAttendeesAsync(fromDate, toDate, count, ct);
        return Ok(result);
    }

    /// <summary>
    /// Employees with attendance rate below a threshold. Defaults to last 30 days, threshold 75%.
    /// </summary>
    [HttpGet("analytics/low-attendees")]
    public async Task<IActionResult> GetLowAttendees(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] double threshold = 75,
        CancellationToken ct = default)
    {
        var toDate = ParseDateOrDefault(to, DateOnly.FromDateTime(DateTime.UtcNow));
        var fromDate = ParseDateOrDefault(from, toDate.AddDays(-29));

        if (fromDate > toDate)
            return BadRequest(new { message = "'from' must be before or equal to 'to'." });
        if (threshold < 0) threshold = 0;
        if (threshold > 100) threshold = 100;

        var result = await _dashboardService.GetLowAttendeesAsync(fromDate, toDate, threshold, ct);
        return Ok(result);
    }

    // ─────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────
    private static DateOnly ParseDateOrDefault(string? value, DateOnly fallback)
    {
        if (!string.IsNullOrWhiteSpace(value) && DateOnly.TryParseExact(value, "yyyy-MM-dd", out var parsed))
            return parsed;
        return fallback;
    }
}
