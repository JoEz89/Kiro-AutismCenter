using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.NameEn)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.NameAr)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.DescriptionEn)
            .HasMaxLength(1000);

        builder.Property(c => c.DescriptionAr)
            .HasMaxLength(1000);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(c => c.NameEn)
            .HasDatabaseName("IX_Categories_NameEn");

        builder.HasIndex(c => c.NameAr)
            .HasDatabaseName("IX_Categories_NameAr");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Categories_IsActive");

        // Navigation properties
        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}