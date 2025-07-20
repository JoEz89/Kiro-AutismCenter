using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.TitleEn)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.TitleAr)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.DescriptionEn)
            .HasColumnType("text");

        builder.Property(c => c.DescriptionAr)
            .HasColumnType("text");

        builder.Property(c => c.DurationInMinutes)
            .IsRequired();

        builder.Property(c => c.ThumbnailUrl)
            .HasMaxLength(500);

        // Configure Price value object
        builder.OwnsOne(c => c.Price, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            price.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .HasDefaultValue("BHD")
                .IsRequired();
        });

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CourseCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(c => c.TitleEn)
            .HasDatabaseName("IX_Courses_TitleEn");

        builder.HasIndex(c => c.TitleAr)
            .HasDatabaseName("IX_Courses_TitleAr");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Courses_IsActive");

        builder.HasIndex(c => c.CourseCode)
            .IsUnique()
            .HasDatabaseName("IX_Courses_CourseCode");

        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Courses_CreatedAt");

        // Navigation properties
        builder.HasMany(c => c.Modules)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Courses_DurationInMinutes", "\"DurationInMinutes\" > 0"));

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}