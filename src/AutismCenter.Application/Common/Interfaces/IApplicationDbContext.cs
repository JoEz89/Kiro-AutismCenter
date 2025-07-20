using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Course> Courses { get; }
    DbSet<CourseModule> CourseModules { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<ModuleProgress> ModuleProgress { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<DoctorAvailability> DoctorAvailabilities { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}