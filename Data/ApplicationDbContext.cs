using FacialRecognitionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacialRecognitionAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<OfficeLocation> OfficeLocations => Set<OfficeLocation>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<JobPosition> Positions => Set<JobPosition>();
    public DbSet<AttendanceStatus> AttendanceStatuses => Set<AttendanceStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new Configurations.EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.AttendanceRecordConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.OfficeLocationConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.JobPositionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.AttendanceStatusConfiguration());
    }
}
