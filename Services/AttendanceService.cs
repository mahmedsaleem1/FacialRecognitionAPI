using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using FacialRecognitionAPI.Services.Interfaces;

namespace FacialRecognitionAPI.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        ILogger<AttendanceService> logger)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _logger = logger;
    }

    public async Task<MarkAttendanceResponse> MarkAttendanceAsync(MarkAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(request.Uuid, out var employeeId))
            throw new ArgumentException("Invalid UUID format.");

        var employee = await _employeeRepo.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("No employee found for this UUID.");

        if (await _attendanceRepo.HasAttendanceTodayAsync(employeeId, cancellationToken))
            throw new ConflictException("Attendance already marked for today.");

        var markedAt = DateTime.UtcNow;

        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            MarkedAt = markedAt,
            Status = "present"
        };

        await _attendanceRepo.AddAsync(record, cancellationToken);
        await _attendanceRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Attendance marked for {Uuid} at {MarkedAt}", employeeId, markedAt);

        return new MarkAttendanceResponse
        {
            AttendanceId = $"att_{record.Id}",
            Uuid = employee.Id.ToString(),
            MarkedAt = markedAt.ToString("O"),
            Status = "present"
        };
    }

    public async Task<List<DailyAttendanceRecord>> GetDailyAttendanceAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var dateUtc = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var records = await _attendanceRepo.GetByDateAsync(dateUtc, cancellationToken);

        return records.Select(r => new DailyAttendanceRecord
        {
            AttendanceId = $"att_{r.Id}",
            Uuid = r.Employee.Id.ToString(),
            FullName = r.Employee.FullName,
            Email = r.Employee.Email,
            Department = r.Employee.Department,
            Position = r.Employee.Position,
            MarkedAt = r.MarkedAt.ToString("O"),
            Status = r.Status
        }).ToList();
    }
}
