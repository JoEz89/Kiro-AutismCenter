using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(d => d.NameEn)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.NameAr)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.SpecialtyEn)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.SpecialtyAr)
            .HasMaxLength(255)
            .IsRequired();

        // Configure Email value object
        builder.OwnsOne(d => d.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();
        });

        // Configure PhoneNumber value object
        builder.OwnsOne(d => d.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20);
        });

        builder.Property(d => d.Biography)
            .HasColumnType("text");

        builder.Property(d => d.IsActive)
            .HasDefaultValue(true);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(d => d.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(d => d.NameEn)
            .HasDatabaseName("IX_Doctors_NameEn");

        builder.HasIndex(d => d.NameAr)
            .HasDatabaseName("IX_Doctors_NameAr");

        builder.HasIndex(d => d.SpecialtyEn)
            .HasDatabaseName("IX_Doctors_SpecialtyEn");

        builder.HasIndex(d => d.SpecialtyAr)
            .HasDatabaseName("IX_Doctors_SpecialtyAr");

        builder.HasIndex(d => d.IsActive)
            .HasDatabaseName("IX_Doctors_IsActive");

        // Navigation properties
        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Availability)
            .WithOne(da => da.Doctor)
            .HasForeignKey(da => da.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(d => d.DomainEvents);
    }
}