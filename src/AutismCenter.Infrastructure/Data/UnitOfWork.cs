using Microsoft.EntityFrameworkCore.Storage;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Repositories;

namespace AutismCenter.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private IOrderRepository? _orders;
    private ICourseRepository? _courses;
    private IEnrollmentRepository? _enrollments;
    private IAppointmentRepository? _appointments;
    private IDoctorRepository? _doctors;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public ICourseRepository Courses => _courses ??= new CourseRepository(_context);
    public IEnrollmentRepository Enrollments => _enrollments ??= new EnrollmentRepository(_context);
    public IAppointmentRepository Appointments => _appointments ??= new AppointmentRepository(_context);
    public IDoctorRepository Doctors => _doctors ??= new DoctorRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}