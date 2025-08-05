using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data.Configurations;

/// <summary>
/// Additional performance-focused indexes for frequently queried data
/// </summary>
public static class PerformanceIndexConfiguration
{
    public static void ConfigurePerformanceIndexes(this ModelBuilder modelBuilder)
    {
        // Composite indexes for common query patterns
        
        // Users - Email lookup with verification status
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Email.Value, u.IsEmailVerified })
            .HasDatabaseName("IX_Users_Email_Verified");

        // Products - Active products by category with stock
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.CategoryId, p.IsActive, p.StockQuantity })
            .HasDatabaseName("IX_Products_Category_Active_Stock");

        // Products - Price range queries
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.Price.Amount, p.IsActive })
            .HasDatabaseName("IX_Products_Price_Active");

        // Orders - User orders by status and date
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.UserId, o.Status, o.CreatedAt })
            .HasDatabaseName("IX_Orders_User_Status_Date");

        // Orders - Payment processing queries
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.PaymentStatus, o.Status, o.CreatedAt })
            .HasDatabaseName("IX_Orders_Payment_Status_Date");

        // Appointments - Doctor availability queries
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.Status })
            .HasDatabaseName("IX_Appointments_Doctor_Date_Status");

        // Appointments - User appointment history
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.UserId, a.AppointmentDate, a.Status })
            .HasDatabaseName("IX_Appointments_User_Date_Status");

        // Doctor Availability - Time slot queries
        modelBuilder.Entity<DoctorAvailability>()
            .HasIndex(da => new { da.DoctorId, da.Date, da.IsAvailable })
            .HasDatabaseName("IX_DoctorAvailability_Doctor_Date_Available");

        // Enrollments - User course access
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.UserId, e.CourseId, e.ExpiryDate })
            .HasDatabaseName("IX_Enrollments_User_Course_Expiry");

        // Enrollments - Active enrollments
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.ExpiryDate, e.IsActive })
            .HasDatabaseName("IX_Enrollments_Expiry_Active")
            .HasFilter("\"ExpiryDate\" > NOW()");

        // Course Modules - Course content ordering
        modelBuilder.Entity<CourseModule>()
            .HasIndex(cm => new { cm.CourseId, cm.OrderIndex })
            .HasDatabaseName("IX_CourseModules_Course_Order");

        // Module Progress - User progress tracking
        modelBuilder.Entity<ModuleProgress>()
            .HasIndex(mp => new { mp.EnrollmentId, mp.ModuleId, mp.IsCompleted })
            .HasDatabaseName("IX_ModuleProgress_Enrollment_Module_Completed");

        // Cart Items - Active cart queries
        modelBuilder.Entity<CartItem>()
            .HasIndex(ci => new { ci.CartId, ci.ProductId })
            .HasDatabaseName("IX_CartItems_Cart_Product");

        // Video Access Logs - Security and analytics
        modelBuilder.Entity<VideoAccessLog>()
            .HasIndex(val => new { val.UserId, val.CourseId, val.AccessedAt })
            .HasDatabaseName("IX_VideoAccessLogs_User_Course_Date");

        // Video Streaming Sessions - Active session management
        modelBuilder.Entity<VideoStreamingSession>()
            .HasIndex(vss => new { vss.UserId, vss.CourseId, vss.ExpiresAt })
            .HasDatabaseName("IX_VideoStreamingSessions_User_Course_Expiry")
            .HasFilter("\"ExpiresAt\" > NOW()");

        // Localized Content - Content retrieval
        modelBuilder.Entity<LocalizedContent>()
            .HasIndex(lc => new { lc.ContentKey, lc.Language })
            .IsUnique()
            .HasDatabaseName("IX_LocalizedContent_Key_Language");

        // Email Templates - Template lookup
        modelBuilder.Entity<EmailTemplate>()
            .HasIndex(et => new { et.TemplateKey, et.Language })
            .IsUnique()
            .HasDatabaseName("IX_EmailTemplates_Key_Language");

        // Refresh Tokens - Token validation
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.UserId, rt.ExpiresAt, rt.IsRevoked })
            .HasDatabaseName("IX_RefreshTokens_User_Expiry_Revoked")
            .HasFilter("\"IsRevoked\" = false AND \"ExpiresAt\" > NOW()");
    }
}