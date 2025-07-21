using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ci => ci.CartId)
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .IsRequired();

        builder.Property(ci => ci.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        // Configure Money value object for UnitPrice
        builder.OwnsOne(ci => ci.UnitPrice, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            price.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .HasDefaultValue("BHD")
                .IsRequired();
        });

        builder.Property(ci => ci.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(ci => ci.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(ci => ci.CartId)
            .HasDatabaseName("IX_CartItems_CartId");

        builder.HasIndex(ci => ci.ProductId)
            .HasDatabaseName("IX_CartItems_ProductId");

        // Unique constraint to prevent duplicate products in the same cart
        builder.HasIndex(ci => new { ci.CartId, ci.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_CartId_ProductId");

        builder.HasIndex(ci => ci.CreatedAt)
            .HasDatabaseName("IX_CartItems_CreatedAt");

        // Navigation properties
        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(ci => ci.DomainEvents);
    }
}