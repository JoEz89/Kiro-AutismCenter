using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(oi => oi.OrderId)
            .IsRequired();

        builder.Property(oi => oi.ProductId)
            .IsRequired();

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        // Configure UnitPrice value object
        builder.OwnsOne(oi => oi.UnitPrice, price =>
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

        builder.Property(oi => oi.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(oi => oi.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(oi => oi.OrderId)
            .HasDatabaseName("IX_OrderItems_OrderId");

        builder.HasIndex(oi => oi.ProductId)
            .HasDatabaseName("IX_OrderItems_ProductId");

        builder.HasIndex(oi => new { oi.OrderId, oi.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_OrderItems_OrderId_ProductId");

        // Navigation properties
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_OrderItems_UnitPrice", "\"UnitPrice\" >= 0"));

        // Ignore domain events
        builder.Ignore(oi => oi.DomainEvents);
    }
}