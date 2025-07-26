using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetByUserAndCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetActiveEnrollmentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetExpiredEnrollmentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetCompletedEnrollmentsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UserEnrolledInCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetActiveEnrollmentAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}