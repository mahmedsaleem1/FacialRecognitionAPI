using FacialRecognitionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacialRecognitionAPI.Data.Configurations;

public class AttendanceStatusConfiguration : IEntityTypeConfiguration<AttendanceStatus>
{
    public void Configure(EntityTypeBuilder<AttendanceStatus> builder)
    {
        builder.ToTable("AttendanceStatuses");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(s => s.Name)
            .IsUnique();

        builder.HasData(
            new AttendanceStatus { Id = 1, Name = "present" },
            new AttendanceStatus { Id = 2, Name = "absent" },
            new AttendanceStatus { Id = 3, Name = "late" }
        );
    }
}
