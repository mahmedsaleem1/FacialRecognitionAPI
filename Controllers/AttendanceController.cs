using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FacialRecognitionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// Mark check-in by submitting a face image.
    /// Supports 1:1 verification (with EmployeeId) or 1:N identification (without EmployeeId).
    /// </summary>
    [HttpPost("check-in")]
    [ProducesResponseType(typeof(ApiResponse<MarkAttendanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIn([FromBody] MarkAttendanceRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed.", GetModelErrors()));

        var result = await _attendanceService.MarkCheckInAsync(request, cancellationToken);

        _logger.LogInformation("Check-in recorded for {EmployeeName} ({EmployeeCode}) with similarity {Score:F4}",
            result.EmployeeName, result.EmployeeCode, result.SimilarityScore);

        return Ok(ApiResponse<MarkAttendanceResponse>.Ok(result, "Check-in recorded successfully."));
    }

    /// <summary>
    /// Mark check-out by submitting a face image.
    /// Supports 1:1 verification (with EmployeeId) or 1:N identification (without EmployeeId).
    /// </summary>
    [HttpPost("check-out")]
    [ProducesResponseType(typeof(ApiResponse<MarkAttendanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed.", GetModelErrors()));

        var result = await _attendanceService.MarkCheckOutAsync(request, cancellationToken);

        _logger.LogInformation("Check-out recorded for {EmployeeName} ({EmployeeCode})",
            result.EmployeeName, result.EmployeeCode);

        return Ok(ApiResponse<MarkAttendanceResponse>.Ok(result, "Check-out recorded successfully."));
    }

    /// <summary>
    /// Get today's attendance records.
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(ApiResponse<List<AttendanceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var records = await _attendanceService.GetTodayAttendanceAsync(cancellationToken);
        return Ok(ApiResponse<List<AttendanceResponse>>.Ok(records));
    }

    /// <summary>
    /// Get today's attendance status for a specific employee.
    /// </summary>
    [HttpGet("today/{employeeId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeToday(Guid employeeId, CancellationToken cancellationToken)
    {
        var record = await _attendanceService.GetEmployeeTodayAsync(employeeId, cancellationToken);

        if (record is null)
            return NotFound(ApiResponse<object>.Fail($"No attendance record found for employee '{employeeId}' today."));

        return Ok(ApiResponse<AttendanceResponse>.Ok(record));
    }

    /// <summary>
    /// Get paginated attendance records with optional filters.
    /// Supports filtering by date range, department, and employee.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecords([FromQuery] AttendanceQueryParams query, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetAttendanceRecordsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<AttendanceResponse>>.Ok(result));
    }

    private List<string> GetModelErrors() =>
        ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
}
