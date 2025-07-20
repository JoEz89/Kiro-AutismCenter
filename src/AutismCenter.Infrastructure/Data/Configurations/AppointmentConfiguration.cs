using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.DoctorId)
            .IsRequired();

        builder.Property(a => a.AppointmentDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.DurationInMinutes)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(AppointmentStatus.Scheduled);

        builder.Property(a => a.ZoomMeetingId)
            .HasMaxLength(255);

        builder.Property(a => a.ZoomJoinUrl)
            .HasMaxLength(500);

        builder.Property(a => a.Notes)
            .HasColumnType("text");

        builder.Property(a => a.AppointmentNumber)
            .HasMaxLength(50)
            .IsRequired();

        // Configure PatientInfo value object
        builder.OwnsOne(a => a.PatientInfo, patient =>
        {
            patient.Property(p => p.PatientName)
                .HasColumnName("PatientName")
                .HasMaxLength(255)
                .IsRequired();

            patient.Property(p => p.PatientAge)
                .HasColumnName("PatientAge")
                .IsRequired();

            patient.Property(p => p.MedicalHistory)
                .HasColumnName("PatientMedicalHistory")
                .HasColumnType("text");

            patient.Property(p => p.CurrentConcerns)
                .HasColumnName("PatientCurrentConcerns")
                .HasColumnType("text");

            patient.Property(p => p.EmergencyContact)
                .HasColumnName("EmergencyContactName")
                .HasMaxLength(255);

            patient.Property(p => p.EmergencyPhone)
                .HasColumnName("EmergencyContactPhone")
                .HasMaxLength(20);
        });

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_Appointments_UserId");

        builder.HasIndex(a => a.DoctorId)
            .HasDatabaseName("IX_Appointments_DoctorId");

        builder.HasIndex(a => a.AppointmentDate)
            .HasDatabaseName("IX_Appointments_AppointmentDate");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_Appointments_Status");

        builder.HasIndex(a => a.AppointmentNumber)
            .IsUnique()
            .HasDatabaseName("IX_Appointments_AppointmentNumber");

        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate })
            .HasDatabaseName("IX_Appointments_DoctorId_AppointmentDate");

        builder.HasIndex(a => a.ZoomMeetingId)
            .HasDatabaseName("IX_Appointments_ZoomMeetingId")
            .HasFilter("\"ZoomMeetingId\" IS NOT NULL");

        // Navigation properties
        builder.HasOne(a => a.User)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_DurationInMinutes", 
            "\"DurationInMinutes\" > 0"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_PatientAge", 
            "\"PatientAge\" >= 0 AND \"PatientAge\" <= 150"));

        builder.ToTable(t => t.HasCheckConstraint("CK_Appointments_Status", 
            "\"Status\" IN ('scheduled', 'confirmed', 'in_progress', 'completed', 'cancelled', 'no_show')"));

        // Ignore domain events
        builder.Ignore(a => a.DomainEvents);
    }
}