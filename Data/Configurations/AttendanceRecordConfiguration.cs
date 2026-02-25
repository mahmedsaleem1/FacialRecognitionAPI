using FacialRecognitionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacialRecognitionAPI.Data.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(a => a.Date)
            .IsRequired();

        builder.Property(a => a.CheckInTime)
            .IsRequired();

        builder.Property(a => a.CheckInSimilarityScore)
            .HasColumnType("real");

        builder.Property(a => a.CheckOutSimilarityScore)
            .HasColumnType("real");

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Composite unique: one check-in per employee per day
        builder.HasIndex(a => new { a.EmployeeId, a.Date })
            .IsUnique();

        // Indexes for analytics queries
        builder.HasIndex(a => a.Date);
        builder.HasIndex(a => a.Status);
    }
}
