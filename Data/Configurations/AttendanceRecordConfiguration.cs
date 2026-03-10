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

        builder.Property(a => a.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("present");

        builder.Property(a => a.MarkedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // One attendance record per employee per day
        builder.HasIndex(a => new { a.EmployeeId, a.MarkedAt })
            .IsUnique()
            .HasFilter(null);

        builder.HasOne(a => a.Employee)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
