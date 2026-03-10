using FacialRecognitionAPI.Models.Entities;

namespace FacialRecognitionAPI.Repositories.Interfaces;

public interface IAttendanceRepository : IRepository<AttendanceRecord>
{
    Task<bool> HasAttendanceTodayAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<List<AttendanceRecord>> GetByDateAsync(DateTime dateUtc, CancellationToken cancellationToken = default);
}
