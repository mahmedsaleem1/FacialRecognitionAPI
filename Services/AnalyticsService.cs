using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using FacialRecognitionAPI.Services.Interfaces;

namespace FacialRecognitionAPI.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        ILogger<AnalyticsService> logger)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _logger = logger;
    }

    public async Task<DashboardOverviewResponse> GetDashboardOverviewAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var totalActive = await _employeeRepo.CountActiveAsync(cancellationToken);
        var todayRecords = await _attendanceRepo.GetByDateWithEmployeeAsync(today, cancellationToken);

        var todayPresent = todayRecords.Count(r => r.Status == AttendanceStatus.Present);
        var todayLate = todayRecords.Count(r => r.Status == AttendanceStatus.Late);
        var todayAbsent = totalActive - todayRecords.Count;

        // Weekly average
        var weeklyAvg = await ComputeAverageAttendanceAsync(weekStart, today, totalActive, cancellationToken);

        // Monthly average
        var monthlyAvg = await ComputeAverageAttendanceAsync(monthStart, today, totalActive, cancellationToken);

        // Department breakdown
        var departments = await _employeeRepo.GetDistinctDepartmentsAsync(cancellationToken);
        var deptBreakdown = new List<DepartmentSummary>();
        foreach (var dept in departments)
        {
            var deptTotal = await _employeeRepo.CountActiveByDepartmentAsync(dept, cancellationToken);
            var deptPresent = await _attendanceRepo.CountByDateAndDepartmentAsync(today, dept, cancellationToken);
            deptBreakdown.Add(new DepartmentSummary
            {
                Department = dept,
                TotalEmployees = deptTotal,
                PresentToday = deptPresent,
                AttendancePercentage = deptTotal > 0 ? Math.Round((double)deptPresent / deptTotal * 100, 1) : 0
            });
        }

        return new DashboardOverviewResponse
        {
            TotalEmployees = totalActive + await _employeeRepo.FindAsync(e => !e.IsActive, cancellationToken).ContinueWith(t => t.Result.Count()),
            ActiveEmployees = totalActive,
            TodayPresent = todayPresent,
            TodayLate = todayLate,
            TodayAbsent = todayAbsent,
            TodayAttendancePercentage = totalActive > 0 ? Math.Round((double)todayRecords.Count / totalActive * 100, 1) : 0,
            WeeklyAverageAttendance = weeklyAvg,
            MonthlyAverageAttendance = monthlyAvg,
            DepartmentBreakdown = deptBreakdown
        };
    }

    public async Task<DailyAttendanceSummary> GetDailySummaryAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var totalActive = await _employeeRepo.CountActiveAsync(cancellationToken);
        var records = await _attendanceRepo.GetByDateWithEmployeeAsync(date, cancellationToken);

        return new DailyAttendanceSummary
        {
            Date = date,
            TotalEmployees = totalActive,
            PresentCount = records.Count(r => r.Status == AttendanceStatus.Present),
            LateCount = records.Count(r => r.Status == AttendanceStatus.Late),
            HalfDayCount = records.Count(r => r.Status == AttendanceStatus.HalfDay),
            AbsentCount = totalActive - records.Count,
            ExcusedCount = records.Count(r => r.Status == AttendanceStatus.Excused),
            AttendancePercentage = totalActive > 0 ? Math.Round((double)records.Count / totalActive * 100, 1) : 0
        };
    }

    public async Task<List<DailyAttendanceSummary>> GetWeeklySummaryAsync(DateOnly weekStart, CancellationToken cancellationToken = default)
    {
        var summaries = new List<DailyAttendanceSummary>();
        for (int i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);
            // Skip weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            summaries.Add(await GetDailySummaryAsync(date, cancellationToken));
        }
        return summaries;
    }

    public async Task<List<EmployeeMonthlyReport>> GetMonthlyReportsAsync(int year, int month, string? department = null, CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepo.GetAllActiveAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(department))
            employees = employees.Where(e => e.Department == department).ToList();

        var reports = new List<EmployeeMonthlyReport>();
        foreach (var emp in employees)
        {
            var report = await BuildMonthlyReportAsync(emp.Id, emp.EmployeeCode, emp.FullName, emp.Department, year, month, cancellationToken);
            reports.Add(report);
        }

        return reports.OrderByDescending(r => r.AttendancePercentage).ToList();
    }

    public async Task<EmployeeMonthlyReport> GetEmployeeMonthlyReportAsync(Guid employeeId, int year, int month, CancellationToken cancellationToken = default)
    {
        var emp = await _employeeRepo.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");

        return await BuildMonthlyReportAsync(emp.Id, emp.EmployeeCode, emp.FullName, emp.Department, year, month, cancellationToken);
    }

    #region Private Helpers

    private async Task<EmployeeMonthlyReport> BuildMonthlyReportAsync(
        Guid employeeId, string code, string name, string? department,
        int year, int month, CancellationToken cancellationToken)
    {
        var monthStart = new DateOnly(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (monthEnd > today) monthEnd = today;

        // Count working days (Mon-Fri)
        int workingDays = 0;
        for (var d = monthStart; d <= monthEnd; d = d.AddDays(1))
        {
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                workingDays++;
        }

        var records = await _attendanceRepo.GetByEmployeeAndDateRangeAsync(employeeId, monthStart, monthEnd, cancellationToken);

        var present = records.Count(r => r.Status == AttendanceStatus.Present);
        var late = records.Count(r => r.Status == AttendanceStatus.Late);
        var halfDay = records.Count(r => r.Status == AttendanceStatus.HalfDay);
        var excused = records.Count(r => r.Status == AttendanceStatus.Excused);
        var absent = workingDays - records.Count;

        // Average check-in/out times
        string? avgCheckIn = null;
        string? avgCheckOut = null;

        if (records.Count > 0)
        {
            var avgCheckInMinutes = records.Average(r => r.CheckInTime.Hour * 60 + r.CheckInTime.Minute);
            avgCheckIn = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(avgCheckInMinutes)).ToString("HH:mm");

            var checkOuts = records.Where(r => r.CheckOutTime.HasValue).ToList();
            if (checkOuts.Count > 0)
            {
                var avgCheckOutMinutes = checkOuts.Average(r => r.CheckOutTime!.Value.Hour * 60 + r.CheckOutTime!.Value.Minute);
                avgCheckOut = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(avgCheckOutMinutes)).ToString("HH:mm");
            }
        }

        return new EmployeeMonthlyReport
        {
            EmployeeId = employeeId,
            EmployeeCode = code,
            FullName = name,
            Department = department,
            Year = year,
            Month = month,
            WorkingDays = workingDays,
            DaysPresent = present,
            DaysLate = late,
            DaysHalfDay = halfDay,
            DaysAbsent = Math.Max(0, absent),
            DaysExcused = excused,
            AttendancePercentage = workingDays > 0 ? Math.Round((double)records.Count / workingDays * 100, 1) : 0,
            AverageCheckInTime = avgCheckIn,
            AverageCheckOutTime = avgCheckOut
        };
    }

    private async Task<double> ComputeAverageAttendanceAsync(DateOnly from, DateOnly to, int totalActive, CancellationToken cancellationToken)
    {
        if (totalActive == 0) return 0;

        int workDays = 0;
        double totalPct = 0;

        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                continue;

            workDays++;
            var count = await _attendanceRepo.CountByDateAsync(d, cancellationToken);
            totalPct += (double)count / totalActive * 100;
        }

        return workDays > 0 ? Math.Round(totalPct / workDays, 1) : 0;
    }

    #endregion
}
