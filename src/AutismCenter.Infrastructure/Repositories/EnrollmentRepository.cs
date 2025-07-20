using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.Infrastructure.Repositories;

public class EnrollmentRepository : BaseRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Enrollment?> GetByUserAndCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Modules.Where(m => m.IsActive))
            .Include(e => e.ModuleProgressList)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId, cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Course)
                .ThenInclude(c => c.Modules.Where(m => m.IsActive))
            .Include(e => e.ModuleProgressList)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.User)
            .Include(e => e.ModuleProgressList)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetActiveEnrollmentsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(e => e.User)
            .Include(e => e.Course)
            .Include(e => e.ModuleProgressList)
            .Where(e => e.IsActive && e.ExpiryDate > now)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetExpiredEnrollmentsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(e => e.User)
            .Include(e => e.Course)
            .Where(e => e.IsActive && e.ExpiryDate <= now)
            .OrderByDescending(e => e.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetCompletedEnrollmentsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.User)
            .Include(e => e.Course)
            .Where(e => e.CompletionDate != null)
            .OrderByDescending(e => e.CompletionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserEnrolledInCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.IsActive, cancellationToken);
    }

    public override async Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Modules.Where(m => m.IsActive))
            .Include(e => e.ModuleProgressList)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Enrollment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.User)
            .Include(e => e.Course)
            .Include(e => e.ModuleProgressList)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync(cancellationToken);
    }
}