using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.NameEn)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.NameAr)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.DescriptionEn)
            .HasColumnType("text");

        builder.Property(p => p.DescriptionAr)
            .HasColumnType("text");

        // Configure Money value object
        builder.OwnsOne(p => p.Price, price =>
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

        builder.Property(p => p.StockQuantity)
            .HasDefaultValue(0);

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.ProductSku)
            .HasMaxLength(50)
            .IsRequired();

        // Configure ImageUrls as JSON array
        builder.Property(p => p.ImageUrls)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly())
            .HasColumnName("ImageUrls")
            .HasColumnType("text");

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(p => p.NameEn)
            .HasDatabaseName("IX_Products_NameEn");

        builder.HasIndex(p => p.NameAr)
            .HasDatabaseName("IX_Products_NameAr");

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Products_IsActive");

        builder.HasIndex(p => p.ProductSku)
            .IsUnique()
            .HasDatabaseName("IX_Products_ProductSku");

        builder.HasIndex(p => p.StockQuantity)
            .HasDatabaseName("IX_Products_StockQuantity");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Products_CreatedAt");

        // Navigation properties
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}