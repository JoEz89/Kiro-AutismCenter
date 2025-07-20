using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.UserId)
            .IsRequired();

        // Configure TotalAmount value object
        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .HasDefaultValue("BHD")
                .IsRequired();
        });

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(OrderStatus.Pending);

        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(o => o.PaymentId)
            .HasMaxLength(255);

        // Configure ShippingAddress value object
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .HasMaxLength(255)
                .IsRequired();

            address.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasColumnName("ShippingState")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("ShippingPostalCode")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Configure BillingAddress value object
        builder.OwnsOne(o => o.BillingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("BillingStreet")
                .HasMaxLength(255)
                .IsRequired();

            address.Property(a => a.City)
                .HasColumnName("BillingCity")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasColumnName("BillingState")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("BillingPostalCode")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("BillingCountry")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(o => o.ShippedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(o => o.DeliveredAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(o => o.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_Orders_OrderNumber");

        builder.HasIndex(o => o.UserId)
            .HasDatabaseName("IX_Orders_UserId");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");

        builder.HasIndex(o => o.PaymentStatus)
            .HasDatabaseName("IX_Orders_PaymentStatus");

        builder.HasIndex(o => o.CreatedAt)
            .HasDatabaseName("IX_Orders_CreatedAt");

        builder.HasIndex(o => o.PaymentId)
            .HasDatabaseName("IX_Orders_PaymentId")
            .HasFilter("\"PaymentId\" IS NOT NULL");

        // Navigation properties
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Orders_Status", 
            "\"Status\" IN ('pending', 'confirmed', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded')"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Orders_PaymentStatus", 
            "\"PaymentStatus\" IN ('pending', 'completed', 'failed', 'refunded')"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Orders_Currency", 
            "\"Currency\" IN ('USD', 'BHD')"));

        // Ignore domain events
        builder.Ignore(o => o.DomainEvents);
    }
}