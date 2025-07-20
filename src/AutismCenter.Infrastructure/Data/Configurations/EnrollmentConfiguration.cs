using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.CourseId)
            .IsRequired();

        builder.Property(e => e.EnrollmentDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.ExpiryDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.ProgressPercentage)
            .HasDefaultValue(0);

        builder.Property(e => e.CompletionDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CertificateUrl)
            .HasMaxLength(500);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_Enrollments_UserId");

        builder.HasIndex(e => e.CourseId)
            .HasDatabaseName("IX_Enrollments_CourseId");

        builder.HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique()
            .HasDatabaseName("IX_Enrollments_UserId_CourseId");

        builder.HasIndex(e => e.EnrollmentDate)
            .HasDatabaseName("IX_Enrollments_EnrollmentDate");

        builder.HasIndex(e => e.ExpiryDate)
            .HasDatabaseName("IX_Enrollments_ExpiryDate");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_Enrollments_IsActive");

        builder.HasIndex(e => e.CompletionDate)
            .HasDatabaseName("IX_Enrollments_CompletionDate")
            .HasFilter("\"CompletionDate\" IS NOT NULL");

        // Navigation properties
        builder.HasOne(e => e.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ModuleProgressList)
            .WithOne(mp => mp.Enrollment)
            .HasForeignKey(mp => mp.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Enrollments_ProgressPercentage", 
            "\"ProgressPercentage\" >= 0 AND \"ProgressPercentage\" <= 100"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Enrollments_ExpiryDate", 
            "\"ExpiryDate\" > \"EnrollmentDate\""));

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}