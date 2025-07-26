using AutismCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutismCenter.Infrastructure.Data.Configurations;

public class VideoAccessLogConfiguration : IEntityTypeConfiguration<VideoAccessLog>
{
    public void Configure(EntityTypeBuilder<VideoAccessLog> builder)
    {
        builder.ToTable("VideoAccessLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ModuleId)
            .IsRequired();

        builder.Property(x => x.AccessTime)
            .IsRequired();

        builder.Property(x => x.AccessGranted)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.SessionId)
            .HasMaxLength(100);

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
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ModuleId);
        builder.HasIndex(x => x.AccessTime);
        builder.HasIndex(x => new { x.UserId, x.AccessGranted, x.AccessTime });
    }
}