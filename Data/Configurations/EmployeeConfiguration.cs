using FacialRecognitionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacialRecognitionAPI.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique();

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);
        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Department)
            .HasMaxLength(100);

        builder.Property(e => e.Position)
            .HasMaxLength(100);

        builder.Property(e => e.CloudinaryImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.CloudinaryPublicId)
            .IsRequired()
            .HasMaxLength(500);

        // Encrypted embedding (max ~4KB for 512-dim float[] encrypted)
        builder.Property(e => e.EncryptedEmbedding)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(e => e.EncryptionIv)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(e => e.EncryptionTag)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes for common queries
        builder.HasIndex(e => e.Department);
        builder.HasIndex(e => e.IsActive);

        // One-to-many: Employee -> AttendanceRecords
        builder.HasMany(e => e.AttendanceRecords)
            .WithOne(a => a.Employee)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
