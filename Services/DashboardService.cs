using FacialRecognitionAPI.Data;
using FacialRecognitionAPI.Models.DTOs.Responses.Dashboard;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacialRecognitionAPI.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApplicationDbContext db, ILogger<DashboardService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ══════════════════════════════════════════════════
    //  Summary
    // ══════════════════════════════════════════════════
    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        var monthStart = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var thirtyDaysAgo = todayUtc.AddDays(-30);

        var totalEmployees = await _db.Employees.CountAsync(ct);
        var todayPresentCount = await _db.AttendanceRecords
            .CountAsync(a => a.MarkedAt >= todayUtc && a.MarkedAt < tomorrowUtc, ct);

        var newEmployeesThisMonth = await _db.Employees
            .CountAsync(e => e.CreatedAt >= monthStart, ct);

        // Average attendance rate last 30 days (exclude weekends - Mon-Fri)
        var last30DaysAttendance = await _db.AttendanceRecords
            .Where(a => a.MarkedAt >= thirtyDaysAgo && a.MarkedAt < tomorrowUtc)
            .GroupBy(a => a.MarkedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var workingDaysLast30 = Enumerable.Range(0, 30)
            .Select(i => thirtyDaysAgo.AddDays(i))
            .Count(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday);

        var avgRate = totalEmployees > 0 && workingDaysLast30 > 0
            ? last30DaysAttendance.Sum(d => d.Count) / (double)(totalEmployees * workingDaysLast30) * 100
            : 0;

        // Recent activity (last 10 attendance marks)
        var recentActivity = await _db.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Employee)
            .OrderByDescending(a => a.MarkedAt)
            .Take(10)
            .Select(a => new RecentActivityItem
            {
                EmployeeId = a.EmployeeId.ToString(),
                FullName = a.Employee.FullName,
                Department = a.Employee.Department,
                MarkedAt = a.MarkedAt.ToString("O"),
                Status = a.Status
            })
            .ToListAsync(ct);

        return new DashboardSummaryResponse
        {
            TotalEmployees = totalEmployees,
            TodayPresentCount = todayPresentCount,
            TodayAbsentCount = totalEmployees - todayPresentCount,
            TodayAttendanceRate = totalEmployees > 0
                ? Math.Round(todayPresentCount / (double)totalEmployees * 100, 2) : 0,
            NewEmployeesThisMonth = newEmployeesThisMonth,
            AverageAttendanceRateLast30Days = Math.Round(avgRate, 2),
            RecentActivity = recentActivity
        };
    }

    // ══════════════════════════════════════════════════
    //  Employees
    // ══════════════════════════════════════════════════
    public async Task<EmployeeListResponse> GetEmployeesAsync(
        string? search, string? department, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var dept = department.Trim().ToLower();
            query = query.Where(e => e.Department != null && e.Department.ToLower() == dept);
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var employees = await query
            .OrderBy(e => e.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                e.Id,
                e.FullName,
                e.Email,
                e.Phone,
                e.Department,
                e.Position,
                e.JoinDate,
                e.CreatedAt,
                TotalAttendanceDays = e.AttendanceRecords.Count(),
                LastAttendanceDate = e.AttendanceRecords
                    .OrderByDescending(a => a.MarkedAt)
                    .Select(a => (DateTime?)a.MarkedAt)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var joinDateBase = DateOnly.FromDateTime(DateTime.UtcNow);

        return new EmployeeListResponse
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            Employees = employees.Select(e =>
            {
                var workingDaysSinceJoin = CountWorkingDays(e.JoinDate, joinDateBase);
                return new EmployeeDetailResponse
                {
                    Uuid = e.Id.ToString(),
                    FullName = e.FullName,
                    Email = e.Email,
                    Phone = e.Phone,
                    Department = e.Department,
                    Position = e.Position,
                    JoinDate = e.JoinDate.ToString("yyyy-MM-dd"),
                    CreatedAt = e.CreatedAt.ToString("O"),
                    TotalAttendanceDays = e.TotalAttendanceDays,
                    AttendanceRate = workingDaysSinceJoin > 0
                        ? Math.Round(e.TotalAttendanceDays / (double)workingDaysSinceJoin * 100, 2) : 0,
                    LastAttendanceDate = e.LastAttendanceDate?.ToString("O")
                };
            }).ToList()
        };
    }

    public async Task<EmployeeDetailResponse> GetEmployeeByIdAsync(Guid employeeId, CancellationToken ct = default)
    {
        var e = await _db.Employees
            .AsNoTracking()
            .Where(emp => emp.Id == employeeId)
            .Select(emp => new
            {
                emp.Id,
                emp.FullName,
                emp.Email,
                emp.Phone,
                emp.Department,
                emp.Position,
                emp.JoinDate,
                emp.CreatedAt,
                TotalAttendanceDays = emp.AttendanceRecords.Count(),
                LastAttendanceDate = emp.AttendanceRecords
                    .OrderByDescending(a => a.MarkedAt)
                    .Select(a => (DateTime?)a.MarkedAt)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Employee not found.");

        var workingDays = CountWorkingDays(e.JoinDate, DateOnly.FromDateTime(DateTime.UtcNow));

        return new EmployeeDetailResponse
        {
            Uuid = e.Id.ToString(),
            FullName = e.FullName,
            Email = e.Email,
            Phone = e.Phone,
            Department = e.Department,
            Position = e.Position,
            JoinDate = e.JoinDate.ToString("yyyy-MM-dd"),
            CreatedAt = e.CreatedAt.ToString("O"),
            TotalAttendanceDays = e.TotalAttendanceDays,
            AttendanceRate = workingDays > 0
                ? Math.Round(e.TotalAttendanceDays / (double)workingDays * 100, 2) : 0,
            LastAttendanceDate = e.LastAttendanceDate?.ToString("O")
        };
    }

    public async Task<List<DepartmentStat>> GetEmployeesByDepartmentAsync(CancellationToken ct = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        return await _db.Employees
            .AsNoTracking()
            .GroupBy(e => e.Department ?? "Unassigned")
            .Select(g => new DepartmentStat
            {
                Department = g.Key,
                EmployeeCount = g.Count(),
                TodayPresentCount = g.Count(e =>
                    e.AttendanceRecords.Any(a => a.MarkedAt >= todayUtc && a.MarkedAt < tomorrowUtc)),
                TodayAbsentCount = g.Count() - g.Count(e =>
                    e.AttendanceRecords.Any(a => a.MarkedAt >= todayUtc && a.MarkedAt < tomorrowUtc)),
                AverageAttendanceRate = 0 // computed below
            })
            .ToListAsync(ct);
    }

    // ══════════════════════════════════════════════════
    //  Attendance
    // ══════════════════════════════════════════════════
    public async Task DeleteAttendanceAsync(Guid attendanceId, CancellationToken ct = default)
    {
        var record = await _db.AttendanceRecords.FindAsync([attendanceId], ct)
            ?? throw new KeyNotFoundException("Attendance record not found.");

        _db.AttendanceRecords.Remove(record);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Attendance record {Id} deleted for employee {EmployeeId}", attendanceId, record.EmployeeId);
    }

    public async Task<AttendanceOverviewResponse> GetAttendanceOverviewAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
        var totalEmployees = await _db.Employees.CountAsync(ct);

        var dailyCounts = await _db.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.MarkedAt >= fromUtc && a.MarkedAt < toUtc)
            .GroupBy(a => a.MarkedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync(ct);

        var workingDays = GetWorkingDays(from, to);

        var summaries = workingDays.Select(d =>
        {
            var count = dailyCounts.FirstOrDefault(c => DateOnly.FromDateTime(c.Date) == d)?.Count ?? 0;
            return new DailyAttendanceSummary
            {
                Date = d.ToString("yyyy-MM-dd"),
                PresentCount = count,
                AbsentCount = totalEmployees - count,
                AttendanceRate = totalEmployees > 0
                    ? Math.Round(count / (double)totalEmployees * 100, 2) : 0
            };
        }).ToList();

        var avgRate = summaries.Count > 0 ? Math.Round(summaries.Average(s => s.AttendanceRate), 2) : 0;

        return new AttendanceOverviewResponse
        {
            FromDate = from.ToString("yyyy-MM-dd"),
            ToDate = to.ToString("yyyy-MM-dd"),
            TotalWorkingDays = summaries.Count,
            TotalEmployees = totalEmployees,
            AverageAttendanceRate = avgRate,
            DailySummaries = summaries
        };
    }

    public async Task<MonthlyAttendanceResponse> GetMonthlyAttendanceAsync(
        int year, int month, CancellationToken ct = default)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        if (to > DateOnly.FromDateTime(DateTime.UtcNow))
            to = DateOnly.FromDateTime(DateTime.UtcNow);

        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
        var workingDays = GetWorkingDays(from, to);
        var workingDayCount = workingDays.Count;

        var employees = await _db.Employees
            .AsNoTracking()
            .Select(e => new
            {
                e.Id,
                e.FullName,
                e.Department,
                DaysPresent = e.AttendanceRecords
                    .Count(a => a.MarkedAt >= fromUtc && a.MarkedAt < toUtc)
            })
            .OrderByDescending(e => e.DaysPresent)
            .ToListAsync(ct);

        var records = employees.Select(e => new EmployeeMonthlyRecord
        {
            EmployeeId = e.Id.ToString(),
            FullName = e.FullName,
            Department = e.Department,
            DaysPresent = e.DaysPresent,
            DaysAbsent = workingDayCount - e.DaysPresent,
            AttendanceRate = workingDayCount > 0
                ? Math.Round(e.DaysPresent / (double)workingDayCount * 100, 2) : 0
        }).ToList();

        return new MonthlyAttendanceResponse
        {
            Year = year,
            Month = month,
            TotalWorkingDays = workingDayCount,
            TotalEmployees = employees.Count,
            AverageAttendanceRate = records.Count > 0
                ? Math.Round(records.Average(r => r.AttendanceRate), 2) : 0,
            Records = records
        };
    }

    public async Task<EmployeeAttendanceHistoryResponse> GetEmployeeAttendanceHistoryAsync(
        Guid employeeId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct)
            ?? throw new KeyNotFoundException("Employee not found.");

        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);

        var records = await _db.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.MarkedAt >= fromUtc && a.MarkedAt < toUtc)
            .OrderByDescending(a => a.MarkedAt)
            .Select(a => new AttendanceHistoryItem
            {
                Date = a.MarkedAt.Date.ToString("yyyy-MM-dd"),
                MarkedAt = a.MarkedAt.ToString("O"),
                Status = a.Status
            })
            .ToListAsync(ct);

        var workingDays = GetWorkingDays(from, to).Count;

        return new EmployeeAttendanceHistoryResponse
        {
            EmployeeId = employeeId.ToString(),
            FullName = employee.FullName,
            Department = employee.Department,
            TotalDaysPresent = records.Count,
            AttendanceRate = workingDays > 0
                ? Math.Round(records.Count / (double)workingDays * 100, 2) : 0,
            History = records
        };
    }

    // ══════════════════════════════════════════════════
    //  Analytics
    // ══════════════════════════════════════════════════
    public async Task<AttendanceTrendResponse> GetAttendanceTrendAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
        var totalEmployees = await _db.Employees.CountAsync(ct);

        var dailyCounts = await _db.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.MarkedAt >= fromUtc && a.MarkedAt < toUtc)
            .GroupBy(a => a.MarkedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync(ct);

        var workingDays = GetWorkingDays(from, to);

        return new AttendanceTrendResponse
        {
            FromDate = from.ToString("yyyy-MM-dd"),
            ToDate = to.ToString("yyyy-MM-dd"),
            DataPoints = workingDays.Select(d =>
            {
                var count = dailyCounts.FirstOrDefault(c => DateOnly.FromDateTime(c.Date) == d)?.Count ?? 0;
                return new TrendDataPoint
                {
                    Date = d.ToString("yyyy-MM-dd"),
                    PresentCount = count,
                    TotalEmployees = totalEmployees,
                    AttendanceRate = totalEmployees > 0
                        ? Math.Round(count / (double)totalEmployees * 100, 2) : 0
                };
            }).ToList()
        };
    }

    public async Task<DepartmentStatsResponse> GetDepartmentStatsAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        var workingDayCount = GetWorkingDays(from, to).Count;

        var departments = await _db.Employees
            .AsNoTracking()
            .GroupBy(e => e.Department ?? "Unassigned")
            .Select(g => new
            {
                Department = g.Key,
                EmployeeCount = g.Count(),
                TotalAttendance = g.Sum(e =>
                    e.AttendanceRecords.Count(a => a.MarkedAt >= fromUtc && a.MarkedAt < toUtc)),
                TodayPresentCount = g.Count(e =>
                    e.AttendanceRecords.Any(a => a.MarkedAt >= todayUtc && a.MarkedAt < tomorrowUtc))
            })
            .ToListAsync(ct);

        return new DepartmentStatsResponse
        {
            Departments = departments.Select(d => new DepartmentStat
            {
                Department = d.Department,
                EmployeeCount = d.EmployeeCount,
                TodayPresentCount = d.TodayPresentCount,
                TodayAbsentCount = d.EmployeeCount - d.TodayPresentCount,
                AverageAttendanceRate = d.EmployeeCount > 0 && workingDayCount > 0
                    ? Math.Round(d.TotalAttendance / (double)(d.EmployeeCount * workingDayCount) * 100, 2)
                    : 0
            }).ToList()
        };
    }

    public async Task<TopAttendeeResponse> GetTopAttendeesAsync(
        DateOnly from, DateOnly to, int count, CancellationToken ct = default)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
        var workingDayCount = GetWorkingDays(from, to).Count;

        var topEmployees = await _db.Employees
            .AsNoTracking()
            .Select(e => new
            {
                e.Id,
                e.FullName,
                e.Department,
                DaysPresent = e.AttendanceRecords
                    .Count(a => a.MarkedAt >= fromUtc && a.MarkedAt < toUtc)
            })
            .OrderByDescending(e => e.DaysPresent)
            .Take(count)
            .ToListAsync(ct);

        return new TopAttendeeResponse
        {
            FromDate = from.ToString("yyyy-MM-dd"),
            ToDate = to.ToString("yyyy-MM-dd"),
            Rankings = topEmployees.Select((e, i) => new AttendeeRanking
            {
                Rank = i + 1,
                EmployeeId = e.Id.ToString(),
                FullName = e.FullName,
                Department = e.Department,
                DaysPresent = e.DaysPresent,
                TotalWorkingDays = workingDayCount,
                AttendanceRate = workingDayCount > 0
                    ? Math.Round(e.DaysPresent / (double)workingDayCount * 100, 2) : 0
            }).ToList()
        };
    }

    public async Task<LowAttendeeResponse> GetLowAttendeesAsync(
        DateOnly from, DateOnly to, double threshold, CancellationToken ct = default)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
        var workingDayCount = GetWorkingDays(from, to).Count;

        var employees = await _db.Employees
            .AsNoTracking()
            .Select(e => new
            {
                e.Id,
                e.FullName,
                e.Department,
                DaysPresent = e.AttendanceRecords
                    .Count(a => a.MarkedAt >= fromUtc && a.MarkedAt < toUtc)
            })
            .ToListAsync(ct);

        var lowAttendees = employees
            .Select(e => new AttendeeRanking
            {
                EmployeeId = e.Id.ToString(),
                FullName = e.FullName,
                Department = e.Department,
                DaysPresent = e.DaysPresent,
                TotalWorkingDays = workingDayCount,
                AttendanceRate = workingDayCount > 0
                    ? Math.Round(e.DaysPresent / (double)workingDayCount * 100, 2) : 0
            })
            .Where(e => e.AttendanceRate < threshold)
            .OrderBy(e => e.AttendanceRate)
            .ToList();

        for (int i = 0; i < lowAttendees.Count; i++)
            lowAttendees[i].Rank = i + 1;

        return new LowAttendeeResponse
        {
            FromDate = from.ToString("yyyy-MM-dd"),
            ToDate = to.ToString("yyyy-MM-dd"),
            Threshold = threshold,
            Employees = lowAttendees
        };
    }

    // ══════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════
    private static List<DateOnly> GetWorkingDays(DateOnly from, DateOnly to)
    {
        var days = new List<DateOnly>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                days.Add(d);
        }
        return days;
    }

    private static int CountWorkingDays(DateOnly from, DateOnly to)
        => GetWorkingDays(from, to).Count;
}
