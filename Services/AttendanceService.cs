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
    private readonly IOfficeLocationRepository _officeLocationRepo;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        IOfficeLocationRepository officeLocationRepo,
        ILogger<AttendanceService> logger)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _officeLocationRepo = officeLocationRepo;
        _logger = logger;
    }

    public async Task<MarkAttendanceResponse> MarkAttendanceAsync(MarkAttendanceRequest request, DateTimeOffset checkInTimestamp, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(request.Uuid, out var employeeId))
            throw new ArgumentException("Invalid UUID format.");

        var employeeExists = await _employeeRepo.AnyAsync(e => e.Id == employeeId, cancellationToken);
        if (!employeeExists)
            throw new KeyNotFoundException("No employee found for this UUID.");

        if (await _attendanceRepo.HasAttendanceTodayAsync(employeeId, cancellationToken))
            throw new ConflictException("Attendance already marked for today.");

        var officeLocation = await _officeLocationRepo.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("Office location is not configured.");

        const int allowedRadiusMeters = 100;

        var distanceMeters = CalculateDistanceMeters(
            request.Latitude,
            request.Longitude,
            (double)officeLocation.Latitude,
            (double)officeLocation.Longitude);

        if (distanceMeters > allowedRadiusMeters)
            throw new InvalidOperationException($"Check-in location is outside the allowed 100 meter office proximity. Distance: {Math.Round(distanceMeters, 2)} meters, allowed: {allowedRadiusMeters} meters.");

        var markedAt = checkInTimestamp.UtcDateTime;

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
            Uuid = employeeId.ToString(),
            MarkedAt = markedAt.ToString("O"),
            Status = "present"
        };
    }

    private static double CalculateDistanceMeters(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double earthRadiusMeters = 6371000d;

        var latitude1Rad = DegreesToRadians(latitude1);
        var latitude2Rad = DegreesToRadians(latitude2);
        var latitudeDeltaRad = DegreesToRadians(latitude2 - latitude1);
        var longitudeDeltaRad = DegreesToRadians(longitude2 - longitude1);

        var a = Math.Sin(latitudeDeltaRad / 2) * Math.Sin(latitudeDeltaRad / 2)
                + Math.Cos(latitude1Rad) * Math.Cos(latitude2Rad)
                * Math.Sin(longitudeDeltaRad / 2) * Math.Sin(longitudeDeltaRad / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

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
