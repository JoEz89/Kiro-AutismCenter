using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByAppointmentNumberAsync(string appointmentNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? doctorId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> GenerateAppointmentNumberAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetAppointmentsForExportAsync(DateTime? startDate, DateTime? endDate, string? status, Guid? doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(int count, CancellationToken cancellationToken = default);
}