using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.ToTable("DoctorAvailabilities");

        builder.HasKey(da => da.Id);

        builder.Property(da => da.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(da => da.DoctorId)
            .IsRequired();

        builder.Property(da => da.DayOfWeek)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(da => da.StartTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(da => da.EndTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(da => da.IsActive)
            .HasDefaultValue(true);

        builder.Property(da => da.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(da => da.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(da => da.DoctorId)
            .HasDatabaseName("IX_DoctorAvailabilities_DoctorId");

        builder.HasIndex(da => new { da.DoctorId, da.DayOfWeek })
            .HasDatabaseName("IX_DoctorAvailabilities_DoctorId_DayOfWeek");

        builder.HasIndex(da => da.IsActive)
            .HasDatabaseName("IX_DoctorAvailabilities_IsActive");

        // Navigation properties
        builder.HasOne(da => da.Doctor)
            .WithMany(d => d.Availability)
            .HasForeignKey(da => da.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_DoctorAvailabilities_Times", 
            "\"StartTime\" < \"EndTime\""));

        // Ignore domain events
        builder.Ignore(da => da.DomainEvents);
    }
}