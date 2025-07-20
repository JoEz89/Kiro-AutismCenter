using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.Infrastructure.Repositories;

public class DoctorRepository : BaseRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Doctor?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Availability.Where(a => a.IsActive))
            .FirstOrDefaultAsync(d => d.Email.Value == email.Value, cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Availability.Where(a => a.IsActive))
            .Where(d => d.IsActive)
            .OrderBy(d => d.NameEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Doctor>> GetAvailableAsync(DateTime dateTime, int durationInMinutes, CancellationToken cancellationToken = default)
    {
        var dayOfWeek = dateTime.DayOfWeek;
        var timeOnly = TimeOnly.FromDateTime(dateTime);
        var endTime = timeOnly.AddMinutes(durationInMinutes);

        return await DbSet
            .Include(d => d.Availability.Where(a => a.IsActive))
            .Include(d => d.Appointments.Where(a => a.Status != Domain.Enums.AppointmentStatus.Cancelled))
            .Where(d => d.IsActive &&
                       d.Availability.Any(a => a.IsActive &&
                                             a.DayOfWeek == dayOfWeek &&
                                             a.StartTime <= timeOnly &&
                                             a.EndTime >= endTime))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(d => d.Email.Value == email.Value, cancellationToken);
    }

    public override async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Availability.Where(a => a.IsActive))
            .Include(d => d.Appointments)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Availability.Where(a => a.IsActive))
            .OrderBy(d => d.NameEn)
            .ToListAsync(cancellationToken);
    }
}