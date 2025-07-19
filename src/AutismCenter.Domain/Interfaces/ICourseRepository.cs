using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Course?> GetByCodeAsync(string courseCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task AddAsync(Course course, CancellationToken cancellationToken = default);
    Task UpdateAsync(Course course, CancellationToken cancellationToken = default);
    Task DeleteAsync(Course course, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string courseCode, CancellationToken cancellationToken = default);
}