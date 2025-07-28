using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("EmailTemplates");

        builder.HasKey(et => et.Id);

        builder.Property(et => et.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(et => et.TemplateKey)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(et => et.Language)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(et => et.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(et => et.Body)
            .IsRequired();

        builder.Property(et => et.Description)
            .HasMaxLength(500);

        builder.Property(et => et.IsActive)
            .HasDefaultValue(true);

        builder.Property(et => et.CreatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(et => et.UpdatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(et => et.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(et => et.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(et => new { et.TemplateKey, et.Language })
            .IsUnique()
            .HasDatabaseName("IX_EmailTemplates_TemplateKey_Language");

        builder.HasIndex(et => et.TemplateKey)
            .HasDatabaseName("IX_EmailTemplates_TemplateKey");

        builder.HasIndex(et => et.Language)
            .HasDatabaseName("IX_EmailTemplates_Language");

        builder.HasIndex(et => et.IsActive)
            .HasDatabaseName("IX_EmailTemplates_IsActive");

        builder.HasIndex(et => et.CreatedAt)
            .HasDatabaseName("IX_EmailTemplates_CreatedAt");

        // Ignore domain events
        builder.Ignore(et => et.DomainEvents);
    }
}