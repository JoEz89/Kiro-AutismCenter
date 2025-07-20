using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.Infrastructure.Repositories;

public class CourseRepository : BaseRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Course?> GetByCodeAsync(string courseCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Modules.Where(m => m.IsActive))
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CourseCode == courseCode, cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Modules.Where(m => m.IsActive))
            .Where(c => c.IsActive)
            .OrderBy(c => c.TitleEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        
        return await DbSet
            .Include(c => c.Modules.Where(m => m.IsActive))
            .Where(c => c.IsActive && 
                       (c.TitleEn.ToLower().Contains(lowerSearchTerm) ||
                        c.TitleAr.ToLower().Contains(lowerSearchTerm) ||
                        c.DescriptionEn.ToLower().Contains(lowerSearchTerm) ||
                        c.DescriptionAr.ToLower().Contains(lowerSearchTerm) ||
                        c.CourseCode.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(c => c.TitleEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string courseCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(c => c.CourseCode == courseCode, cancellationToken);
    }

    public override async Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Modules.Where(m => m.IsActive))
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Modules.Where(m => m.IsActive))
            .OrderBy(c => c.TitleEn)
            .ToListAsync(cancellationToken);
    }
}