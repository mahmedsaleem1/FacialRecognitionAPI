using FacialRecognitionAPI.Data;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacialRecognitionAPI.Repositories;

public class AttendanceRepository : Repository<AttendanceRecord>, IAttendanceRepository
{
    public AttendanceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<AttendanceRecord?> GetTodayRecordAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet.FirstOrDefaultAsync(
            a => a.EmployeeId == employeeId && a.Date == today, cancellationToken);
    }

    public async Task<AttendanceRecord?> GetRecordAsync(Guid employeeId, DateOnly date, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date, cancellationToken);

    public async Task<List<AttendanceRecord>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => await _dbSet
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.Date >= from && a.Date <= to)
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.CheckInTime)
            .ToListAsync(cancellationToken);

    public async Task<List<AttendanceRecord>> GetByEmployeeAndDateRangeAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => await _dbSet
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.Date >= from && a.Date <= to)
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);

    public async Task<int> CountByDateAndStatusAsync(DateOnly date, AttendanceStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(a => a.Date == date);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(a => a.Date == date, cancellationToken);

    public async Task<int> CountByDateAndDepartmentAsync(DateOnly date, string department, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(
            a => a.Date == date && a.Employee.Department == department, cancellationToken);

    public async Task<List<AttendanceRecord>> GetByDateWithEmployeeAsync(DateOnly date, CancellationToken cancellationToken = default)
        => await _dbSet
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.Date == date)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync(cancellationToken);

    public async Task<(int Total, List<AttendanceRecord> Items)> GetPagedAsync(
        DateOnly? from, DateOnly? to, string? department, Guid? employeeId,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Include(a => a.Employee).AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.Date <= to.Value);
        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(a => a.Employee.Department == department);
        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.CheckInTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (total, items);
    }
}
