using Microsoft.EntityFrameworkCore;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Enums;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.Infrastructure.Repositories;

public class AppointmentRepository : BaseRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Appointment?> GetByAppointmentNumberAsync(string appointmentNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.AppointmentNumber == appointmentNumber, cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Doctor)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.User)
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .Where(a => a.Status == status)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? doctorId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate);

        if (doctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == doctorId.Value);
        }

        return await query
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate > now && 
                       (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed))
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        
        return await DbSet
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow &&
                       a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime startTime, DateTime endTime, 
        Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(a => a.DoctorId == doctorId &&
                       a.Status != AppointmentStatus.Cancelled &&
                       a.AppointmentDate < endTime &&
                       a.AppointmentDate.AddMinutes(a.DurationInMinutes) > startTime);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<string> GenerateAppointmentNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"APT-{year}-";
        
        // Get the highest appointment number for the current year
        var lastAppointmentNumber = await DbSet
            .Where(a => a.AppointmentNumber.StartsWith(prefix))
            .OrderByDescending(a => a.AppointmentNumber)
            .Select(a => a.AppointmentNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        if (lastAppointmentNumber != null)
        {
            var numberPart = lastAppointmentNumber.Substring(prefix.Length);
            if (int.TryParse(numberPart, out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D6}"; // Format as 6-digit number with leading zeros
    }

    public override async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.User)
            .Include(a => a.Doctor)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }
}