using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class CourseModuleConfiguration : IEntityTypeConfiguration<CourseModule>
{
    public void Configure(EntityTypeBuilder<CourseModule> builder)
    {
        builder.ToTable("CourseModules");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(cm => cm.CourseId)
            .IsRequired();

        builder.Property(cm => cm.TitleEn)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(cm => cm.TitleAr)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(cm => cm.DescriptionEn)
            .HasMaxLength(1000);

        builder.Property(cm => cm.DescriptionAr)
            .HasMaxLength(1000);

        builder.Property(cm => cm.VideoUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cm => cm.DurationInMinutes)
            .IsRequired();

        builder.Property(cm => cm.Order)
            .IsRequired();

        builder.Property(cm => cm.IsActive)
            .HasDefaultValue(true);

        builder.Property(cm => cm.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(cm => cm.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(cm => cm.CourseId)
            .HasDatabaseName("IX_CourseModules_CourseId");

        builder.HasIndex(cm => new { cm.CourseId, cm.Order })
            .IsUnique()
            .HasDatabaseName("IX_CourseModules_CourseId_Order");

        builder.HasIndex(cm => cm.IsActive)
            .HasDatabaseName("IX_CourseModules_IsActive");

        // Navigation properties
        builder.HasOne(cm => cm.Course)
            .WithMany(c => c.Modules)
            .HasForeignKey(cm => cm.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_CourseModules_DurationInMinutes", "\"DurationInMinutes\" > 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_CourseModules_Order", "\"Order\" > 0"));

        // Ignore domain events
        builder.Ignore(cm => cm.DomainEvents);
    }
}