using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Doctor?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetAvailableAsync(DateTime dateTime, int durationInMinutes, CancellationToken cancellationToken = default);
    Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default);
    Task UpdateAsync(Doctor doctor, CancellationToken cancellationToken = default);
    Task DeleteAsync(Doctor doctor, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);
}