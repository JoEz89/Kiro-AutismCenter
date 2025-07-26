using AutismCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class VideoStreamingSessionConfiguration : IEntityTypeConfiguration<VideoStreamingSession>
{
    public void Configure(EntityTypeBuilder<VideoStreamingSession> builder)
    {
        builder.ToTable("VideoStreamingSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ModuleId)
            .IsRequired();

        builder.Property(x => x.SessionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime);

        builder.Property(x => x.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.SessionId)
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.ModuleId, x.IsActive });

        builder.HasIndex(x => x.StartTime);
    }
}