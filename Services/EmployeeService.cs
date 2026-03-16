using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Data;
using Microsoft.EntityFrameworkCore;
using FacialRecognitionAPI.Repositories.Interfaces;
using FacialRecognitionAPI.Services.Interfaces;

namespace FacialRecognitionAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository employeeRepo, ApplicationDbContext db, IWebHostEnvironment env, ILogger<EmployeeService> logger)
    {
        _employeeRepo = employeeRepo;
        _db = db;
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
            DepartmentId = await ResolveDepartmentIdAsync(form.Department.Trim(), cancellationToken),
            PositionId = await ResolvePositionIdAsync(form.Position.Trim(), cancellationToken),
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
            Department = form.Department.Trim(),
            Position = form.Position.Trim(),
            JoinDate = employee.JoinDate.ToString("yyyy-MM-dd"),
            CreatedAt = employee.CreatedAt.ToString("O")
        };
    }

    private async Task<int?> ResolveDepartmentIdAsync(string departmentName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(departmentName))
            return null;

        var existing = await _db.Departments
            .FirstOrDefaultAsync(d => d.Name.ToLower() == departmentName.ToLower(), cancellationToken);

        if (existing is not null)
            return existing.Id;

        var department = new Department { Name = departmentName };
        _db.Departments.Add(department);
        await _db.SaveChangesAsync(cancellationToken);
        return department.Id;
    }

    private async Task<int?> ResolvePositionIdAsync(string positionName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(positionName))
            return null;

        var existing = await _db.Positions
            .FirstOrDefaultAsync(p => p.Name.ToLower() == positionName.ToLower(), cancellationToken);

        if (existing is not null)
            return existing.Id;

        var position = new JobPosition { Name = positionName };
        _db.Positions.Add(position);
        await _db.SaveChangesAsync(cancellationToken);
        return position.Id;
    }
}
