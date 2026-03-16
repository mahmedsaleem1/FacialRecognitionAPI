using FacialRecognitionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacialRecognitionAPI.Data.Configurations;

public class OfficeLocationConfiguration : IEntityTypeConfiguration<OfficeLocation>
{
    public void Configure(EntityTypeBuilder<OfficeLocation> builder)
    {
        builder.ToTable("OfficeLocations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(o => o.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.Latitude)
            .HasColumnType("decimal(9,6)")
            .IsRequired();

        builder.Property(o => o.Longitude)
            .HasColumnType("decimal(9,6)")
            .IsRequired();

        builder.Property(o => o.AllowedRadiusMeters)
            .HasDefaultValue(100)
            .IsRequired();

        builder.Property(o => o.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(o => o.IsActive);
    }
}
