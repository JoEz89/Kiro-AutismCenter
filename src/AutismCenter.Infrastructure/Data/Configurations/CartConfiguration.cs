using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.ExpiresAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("IX_Carts_UserId");

        builder.HasIndex(c => c.ExpiresAt)
            .HasDatabaseName("IX_Carts_ExpiresAt");

        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Carts_CreatedAt");

        // Navigation properties
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Items)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}