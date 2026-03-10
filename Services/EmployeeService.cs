using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using FacialRecognitionAPI.Services.Interfaces;

namespace FacialRecognitionAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository employeeRepo, IWebHostEnvironment env, ILogger<EmployeeService> logger)
    {
        _employeeRepo = employeeRepo;
        _env = env;
        _logger = logger;
    }

    public async Task<RegisterEmployeeResponse> RegisterAsync(RegisterEmployeeFormRequest form, DateOnly joinDate, CancellationToken cancellationToken = default)
    {
        var emailLower = form.Email.Trim().ToLowerInvariant();

        if (await _employeeRepo.EmailExistsAsync(emailLower, cancellationToken))
            throw new ConflictException("An employee with this email already exists.");

        var id = Guid.NewGuid();

        string? faceImagePath = null;
        if (form.FaceImage is { Length: > 0 })
        {
            var imagesDir = Path.Combine(_env.ContentRootPath, "Images", "employees");
            Directory.CreateDirectory(imagesDir);
            var ext = Path.GetExtension(form.FaceImage.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";
            var fileName = $"{id}{ext}";
            var filePath = Path.Combine(imagesDir, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await form.FaceImage.CopyToAsync(stream, cancellationToken);
            faceImagePath = Path.Combine("Images", "employees", fileName);
        }

        var employee = new Employee
        {
            Id = id,
            FullName = form.FullName.Trim(),
            Email = emailLower,
            Phone = form.Phone.Trim(),
            Department = form.Department.Trim(),
            Position = form.Position.Trim(),
            JoinDate = joinDate,
            FaceImagePath = faceImagePath,
            CreatedAt = DateTime.UtcNow
        };

        await _employeeRepo.AddAsync(employee, cancellationToken);
        await _employeeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee registered: {Id} - {Name} ({Email})", employee.Id, employee.FullName, employee.Email);

        return new RegisterEmployeeResponse
        {
            Uuid = employee.Id.ToString(),
            FullName = employee.FullName,
            Email = employee.Email,
            Phone = employee.Phone,
            Department = employee.Department,
            Position = employee.Position,
            JoinDate = employee.JoinDate.ToString("yyyy-MM-dd"),
            CreatedAt = employee.CreatedAt.ToString("O")
        };
    }
}
