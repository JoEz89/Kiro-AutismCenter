using Microsoft.EntityFrameworkCore;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Common;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> CourseModules => Set<CourseModule>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<ModuleProgress> ModuleProgress => Set<ModuleProgress>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdateTimestamp();
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}