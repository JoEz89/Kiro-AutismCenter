using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class LocalizedContentConfiguration : IEntityTypeConfiguration<LocalizedContent>
{
    public void Configure(EntityTypeBuilder<LocalizedContent> builder)
    {
        builder.ToTable("LocalizedContents");

        builder.HasKey(lc => lc.Id);

        builder.Property(lc => lc.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(lc => lc.Key)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(lc => lc.Language)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(lc => lc.Content)
            .IsRequired();

        builder.Property(lc => lc.Description)
            .HasMaxLength(500);

        builder.Property(lc => lc.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(lc => lc.IsActive)
            .HasDefaultValue(true);

        builder.Property(lc => lc.CreatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(lc => lc.UpdatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(lc => lc.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(lc => lc.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(lc => new { lc.Key, lc.Language })
            .IsUnique()
            .HasDatabaseName("IX_LocalizedContents_Key_Language");

        builder.HasIndex(lc => lc.Category)
            .HasDatabaseName("IX_LocalizedContents_Category");

        builder.HasIndex(lc => lc.Language)
            .HasDatabaseName("IX_LocalizedContents_Language");

        builder.HasIndex(lc => lc.IsActive)
            .HasDatabaseName("IX_LocalizedContents_IsActive");

        builder.HasIndex(lc => lc.CreatedAt)
            .HasDatabaseName("IX_LocalizedContents_CreatedAt");

        // Ignore domain events
        builder.Ignore(lc => lc.DomainEvents);
    }
}