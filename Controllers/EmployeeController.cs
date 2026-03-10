using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FacialRecognitionAPI.Controllers;

[ApiController]
[Route("api/employees")]
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

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Register([FromForm] RegisterEmployeeFormRequest form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid request.";
            return BadRequest(new { message = firstError });
        }

        if (!DateOnly.TryParseExact(form.JoinDate, "yyyy-MM-dd", out var joinDate))
            return BadRequest(new { message = "joinDate must be in YYYY-MM-DD format." });

        var response = await _employeeService.RegisterAsync(form, joinDate, cancellationToken);
        _logger.LogInformation("Employee registered: {Uuid} - {FullName}", response.Uuid, response.FullName);
        return Created(string.Empty, response);
    }
}
