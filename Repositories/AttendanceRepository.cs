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
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        return await _dbSet.AnyAsync(
            a => a.EmployeeId == employeeId && a.MarkedAt >= todayUtc && a.MarkedAt < tomorrowUtc,
            cancellationToken);
    }

    public async Task<List<AttendanceRecord>> GetByDateAsync(DateTime dateUtc, CancellationToken cancellationToken = default)
    {
        var start = dateUtc.Date;
        var end = start.AddDays(1);
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.MarkedAt >= start && a.MarkedAt < end)
            .OrderBy(a => a.MarkedAt)
            .ToListAsync(cancellationToken);
    }
}
