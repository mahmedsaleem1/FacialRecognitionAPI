using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using FacialRecognitionAPI.Services.Interfaces;

namespace FacialRecognitionAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IFacialRecognitionService _faceService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepo,
        ICloudinaryService cloudinaryService,
        IFacialRecognitionService faceService,
        IEncryptionService encryptionService,
        ILogger<EmployeeService> logger)
    {
        _employeeRepo = employeeRepo;
        _cloudinaryService = cloudinaryService;
        _faceService = faceService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<EmployeeResponse> OnboardAsync(OnboardEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        // Check duplicate email
        if (await _employeeRepo.EmailExistsAsync(request.Email.ToLowerInvariant(), cancellationToken))
            throw new InvalidOperationException("An employee with this email already exists.");

        // Decode face image
        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(request.FaceImageBase64);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid Base64 face image data.");
        }

        // Extract face embedding
        var embedding = await _faceService.ExtractFaceEmbeddingAsync(imageBytes)
            ?? throw new InvalidOperationException("No face detected in the provided image. Please provide a clear face photo.");

        // Generate unique employee code
        var employeeCode = GenerateEmployeeCode();

        // Upload image to Cloudinary
        var (imageUrl, publicId) = await _cloudinaryService.UploadImageAsync(imageBytes, $"emp-{employeeCode}");

        // Encrypt face embedding
        var (cipherText, iv, tag) = _encryptionService.Encrypt(embedding);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = employeeCode,
            FullName = request.FullName.Trim(),
            Email = request.Email.ToLowerInvariant(),
            Phone = request.Phone?.Trim(),
            Department = request.Department?.Trim(),
            Position = request.Position?.Trim(),
            CloudinaryImageUrl = imageUrl,
            CloudinaryPublicId = publicId,
            EncryptedEmbedding = cipherText,
            EncryptionIv = iv,
            EncryptionTag = tag,
            IsActive = true,
            JoinDate = request.JoinDate?.ToUniversalTime() ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _employeeRepo.AddAsync(employee, cancellationToken);
        await _employeeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee onboarded: {Code} - {Name} ({Email})", employee.EmployeeCode, employee.FullName, employee.Email);

        return MapToResponse(employee);
    }

    public async Task<EmployeeResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var emp = await _employeeRepo.GetByIdAsync(id, cancellationToken);
        return emp == null ? null : MapToResponse(emp);
    }

    public async Task<EmployeeResponse?> GetByCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
    {
        var emp = await _employeeRepo.GetByCodeAsync(employeeCode, cancellationToken);
        return emp == null ? null : MapToResponse(emp);
    }

    public async Task<List<EmployeeResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepo.GetAllActiveAsync(cancellationToken);
        return employees.Select(MapToResponse).ToList();
    }

    public async Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        if (request.FullName != null) employee.FullName = request.FullName.Trim();
        if (request.Email != null)
        {
            var emailLower = request.Email.ToLowerInvariant();
            if (emailLower != employee.Email && await _employeeRepo.EmailExistsAsync(emailLower, cancellationToken))
                throw new InvalidOperationException("An employee with this email already exists.");
            employee.Email = emailLower;
        }
        if (request.Phone != null) employee.Phone = request.Phone.Trim();
        if (request.Department != null) employee.Department = request.Department.Trim();
        if (request.Position != null) employee.Position = request.Position.Trim();
        if (request.IsActive.HasValue) employee.IsActive = request.IsActive.Value;

        _employeeRepo.Update(employee);
        await _employeeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee updated: {Code}", employee.EmployeeCode);
        return MapToResponse(employee);
    }

    public async Task<EmployeeResponse> UpdateFaceAsync(Guid id, UpdateEmployeeFaceRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(request.FaceImageBase64);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid Base64 face image data.");
        }

        // Extract new face embedding
        var embedding = await _faceService.ExtractFaceEmbeddingAsync(imageBytes)
            ?? throw new InvalidOperationException("No face detected in the provided image.");

        // Delete old Cloudinary image and upload new one
        await _cloudinaryService.DeleteImageAsync(employee.CloudinaryPublicId);
        var (imageUrl, publicId) = await _cloudinaryService.UploadImageAsync(imageBytes, $"emp-{employee.EmployeeCode}");

        // Re-encrypt embedding
        var (cipherText, iv, tag) = _encryptionService.Encrypt(embedding);

        employee.CloudinaryImageUrl = imageUrl;
        employee.CloudinaryPublicId = publicId;
        employee.EncryptedEmbedding = cipherText;
        employee.EncryptionIv = iv;
        employee.EncryptionTag = tag;

        _employeeRepo.Update(employee);
        await _employeeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Face updated for employee {Code}", employee.EmployeeCode);
        return MapToResponse(employee);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        employee.IsActive = false;
        _employeeRepo.Update(employee);
        await _employeeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee deactivated: {Code}", employee.EmployeeCode);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        // Delete Cloudinary image
        await _cloudinaryService.DeleteImageAsync(employee.CloudinaryPublicId);

        _employeeRepo.Remove(employee);
        await _employeeRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee deleted: {Code} - {Name}", employee.EmployeeCode, employee.FullName);
    }

    /// <summary>
    /// Generate a unique employee code like "INT-20260225-A3F7".
    /// </summary>
    private static string GenerateEmployeeCode()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        return $"INT-{datePart}-{randomPart}";
    }

    private static EmployeeResponse MapToResponse(Employee emp) => new()
    {
        Id = emp.Id,
        EmployeeCode = emp.EmployeeCode,
        FullName = emp.FullName,
        Email = emp.Email,
        Phone = emp.Phone,
        Department = emp.Department,
        Position = emp.Position,
        CloudinaryImageUrl = emp.CloudinaryImageUrl,
        IsActive = emp.IsActive,
        JoinDate = emp.JoinDate,
        CreatedAt = emp.CreatedAt
    };
}
