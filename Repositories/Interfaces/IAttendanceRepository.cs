using FacialRecognitionAPI.Models.Entities;

namespace FacialRecognitionAPI.Repositories.Interfaces;

public interface IAttendanceRepository : IRepository<AttendanceRecord>
{
    Task<AttendanceRecord?> GetTodayRecordAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetRecordAsync(Guid employeeId, DateOnly date, CancellationToken cancellationToken = default);
    Task<List<AttendanceRecord>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<AttendanceRecord>> GetByEmployeeAndDateRangeAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<int> CountByDateAndStatusAsync(DateOnly date, AttendanceStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> CountByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<int> CountByDateAndDepartmentAsync(DateOnly date, string department, CancellationToken cancellationToken = default);
    Task<List<AttendanceRecord>> GetByDateWithEmployeeAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<(int Total, List<AttendanceRecord> Items)> GetPagedAsync(DateOnly? from, DateOnly? to, string? department, Guid? employeeId, int page, int pageSize, CancellationToken cancellationToken = default);
}
