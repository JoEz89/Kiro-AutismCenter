using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    ICourseRepository Courses { get; }
    IEnrollmentRepository Enrollments { get; }
    IAppointmentRepository Appointments { get; }
    IDoctorRepository Doctors { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}