using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Configure Email value object
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();
        });

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.PreferredLanguage)
            .HasConversion<string>()
            .HasMaxLength(2)
            .HasDefaultValue(Language.English);

        builder.Property(u => u.IsEmailVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.GoogleId)
            .HasMaxLength(255);

        // Configure PhoneNumber value object
        builder.OwnsOne(u => u.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20);
        });

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(u => u.GoogleId)
            .IsUnique()
            .HasDatabaseName("IX_Users_GoogleId")
            .HasFilter("\"GoogleId\" IS NOT NULL");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("IX_Users_Role");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_Users_CreatedAt");

        // Navigation properties
        builder.HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Enrollments)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Appointments)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}