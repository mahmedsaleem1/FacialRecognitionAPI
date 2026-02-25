using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FacialRecognitionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Onboard a new employee with facial data.
    /// Accepts a base64-encoded face image which is processed, stored in Cloudinary,
    /// and the face embedding is encrypted and saved.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Onboard([FromBody] OnboardEmployeeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed.", GetModelErrors()));

        var employee = await _employeeService.OnboardAsync(request, cancellationToken);

        _logger.LogInformation("Employee onboarded: {EmployeeCode} - {FullName}", employee.EmployeeCode, employee.FullName);

        return CreatedAtAction(
            nameof(GetById),
            new { id = employee.Id },
            ApiResponse<EmployeeResponse>.Ok(employee, "Employee onboarded successfully."));
    }

    /// <summary>
    /// Get all active employees.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllActiveAsync(cancellationToken);
        return Ok(ApiResponse<List<EmployeeResponse>>.Ok(employees));
    }

    /// <summary>
    /// Get a specific employee by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(id, cancellationToken);

        if (employee is null)
            return NotFound(ApiResponse<object>.Fail($"Employee with ID '{id}' not found."));

        return Ok(ApiResponse<EmployeeResponse>.Ok(employee));
    }

    /// <summary>
    /// Get a specific employee by their employee code.
    /// </summary>
    [HttpGet("code/{employeeCode}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string employeeCode, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByCodeAsync(employeeCode, cancellationToken);

        if (employee is null)
            return NotFound(ApiResponse<object>.Fail($"Employee with code '{employeeCode}' not found."));

        return Ok(ApiResponse<EmployeeResponse>.Ok(employee));
    }

    /// <summary>
    /// Update an employee's profile information (not the face data).
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed.", GetModelErrors()));

        var employee = await _employeeService.UpdateAsync(id, request, cancellationToken);

        _logger.LogInformation("Employee updated: {EmployeeId}", id);

        return Ok(ApiResponse<EmployeeResponse>.Ok(employee, "Employee updated successfully."));
    }

    /// <summary>
    /// Update an employee's face image and re-extract the embedding.
    /// </summary>
    [HttpPut("{id:guid}/face")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFace(Guid id, [FromBody] UpdateEmployeeFaceRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed.", GetModelErrors()));

        var employee = await _employeeService.UpdateFaceAsync(id, request, cancellationToken);

        _logger.LogInformation("Face updated for employee: {EmployeeId}", id);

        return Ok(ApiResponse<EmployeeResponse>.Ok(employee, "Face data updated successfully."));
    }

    /// <summary>
    /// Deactivate an employee (soft delete — keeps records but marks inactive).
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _employeeService.DeactivateAsync(id, cancellationToken);

        _logger.LogInformation("Employee deactivated: {EmployeeId}", id);

        return Ok(ApiResponse<string>.Ok(string.Empty, "Employee deactivated successfully."));
    }

    /// <summary>
    /// Permanently delete an employee and their Cloudinary image.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _employeeService.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Employee deleted: {EmployeeId}", id);

        return Ok(ApiResponse<string>.Ok(string.Empty, "Employee deleted successfully."));
    }

    private List<string> GetModelErrors() =>
        ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
}
