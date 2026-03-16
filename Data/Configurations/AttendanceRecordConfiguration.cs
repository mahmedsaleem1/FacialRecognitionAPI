using FacialRecognitionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacialRecognitionAPI.Data.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("Attendance");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(a => a.AttendanceDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(a => a.AttendanceStatusId)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(a => a.MarkedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(a => a.AttendanceDate);
        builder.HasIndex(a => a.AttendanceStatusId);

        // One attendance record per employee per day
        builder.HasIndex(a => new { a.EmployeeId, a.AttendanceDate })
            .IsUnique();

        builder.HasOne(a => a.Employee)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.AttendanceStatus)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(a => a.AttendanceStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
