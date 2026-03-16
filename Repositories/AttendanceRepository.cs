using FacialRecognitionAPI.Data;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacialRecognitionAPI.Repositories;

public class AttendanceRepository : Repository<AttendanceRecord>, IAttendanceRepository
{
    public AttendanceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> HasAttendanceTodayAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet.AnyAsync(
            a => a.EmployeeId == employeeId && a.AttendanceDate == todayUtc,
            cancellationToken);
    }

    public async Task<List<AttendanceRecord>> GetByDateAsync(DateTime dateUtc, CancellationToken cancellationToken = default)
    {
        var day = DateOnly.FromDateTime(dateUtc);
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.AttendanceStatus)
            .Include(a => a.Employee.DepartmentLookup)
            .Include(a => a.Employee.PositionLookup)
            .Where(a => a.AttendanceDate == day)
            .OrderBy(a => a.MarkedAt)
            .ToListAsync(cancellationToken);
    }
}
