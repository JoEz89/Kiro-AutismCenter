using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class ModuleProgressConfiguration : IEntityTypeConfiguration<ModuleProgress>
{
    public void Configure(EntityTypeBuilder<ModuleProgress> builder)
    {
        builder.ToTable("ModuleProgress");

        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(mp => mp.EnrollmentId)
            .IsRequired();

        builder.Property(mp => mp.ModuleId)
            .IsRequired();

        builder.Property(mp => mp.ProgressPercentage)
            .HasDefaultValue(0);

        builder.Property(mp => mp.CompletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(mp => mp.WatchTimeInSeconds)
            .HasDefaultValue(0);

        builder.Property(mp => mp.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(mp => mp.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(mp => mp.EnrollmentId)
            .HasDatabaseName("IX_ModuleProgress_EnrollmentId");

        builder.HasIndex(mp => mp.ModuleId)
            .HasDatabaseName("IX_ModuleProgress_ModuleId");

        builder.HasIndex(mp => new { mp.EnrollmentId, mp.ModuleId })
            .IsUnique()
            .HasDatabaseName("IX_ModuleProgress_EnrollmentId_ModuleId");

        builder.HasIndex(mp => mp.ProgressPercentage)
            .HasDatabaseName("IX_ModuleProgress_ProgressPercentage");

        builder.HasIndex(mp => mp.CompletedAt)
            .HasDatabaseName("IX_ModuleProgress_CompletedAt")
            .HasFilter("\"CompletedAt\" IS NOT NULL");

        // Navigation properties
        builder.HasOne(mp => mp.Enrollment)
            .WithMany(e => e.ModuleProgressList)
            .HasForeignKey(mp => mp.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mp => mp.Module)
            .WithMany()
            .HasForeignKey(mp => mp.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_ModuleProgress_ProgressPercentage", 
            "\"ProgressPercentage\" >= 0 AND \"ProgressPercentage\" <= 100"));

        builder.ToTable(t => t.HasCheckConstraint("CK_ModuleProgress_WatchTimeInSeconds", 
            "\"WatchTimeInSeconds\" >= 0"));

        // Ignore domain events
        builder.Ignore(mp => mp.DomainEvents);
    }
}