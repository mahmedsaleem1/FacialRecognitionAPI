using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FacialRecognitionAPI.Controllers;

[ApiController]
[Route("api/attendance")]
[Produces("application/json")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    /// <summary>
    /// Record attendance for an employee identified by their UUID.
    /// Called only after on-device face recognition succeeds.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid request.";
            return BadRequest(new { message = firstError });
        }

        var response = await _attendanceService.MarkAttendanceAsync(request, cancellationToken);
        _logger.LogInformation("Attendance marked: {AttendanceId} for UUID {Uuid}", response.AttendanceId, response.Uuid);
        return Created(string.Empty, response);
    }

    /// <summary>
    /// Get attendance records for a given date (defaults to today).
    /// Returns all employees who marked attendance on that day.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDailyAttendance([FromQuery] string? date, CancellationToken cancellationToken)
    {
        DateOnly day;
        if (string.IsNullOrWhiteSpace(date))
        {
            day = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out day))
        {
            return BadRequest(new { message = "date must be in YYYY-MM-DD format." });
        }

        var records = await _attendanceService.GetDailyAttendanceAsync(day, cancellationToken);
        return Ok(new { date = day.ToString("yyyy-MM-dd"), count = records.Count, records });
    }
}
