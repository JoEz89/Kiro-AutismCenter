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
    Task<CourseModule?> GetModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Course> Items, int TotalCount)> GetCoursesWithDetailsAsync(int pageNumber, int pageSize, bool? isActive, Guid? categoryId, string? searchTerm, string sortBy, bool sortDescending, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetCoursesByCategoryAsync(Guid? categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetCoursesForExportAsync(bool? isActive, Guid? categoryId, CancellationToken cancellationToken = default);
}