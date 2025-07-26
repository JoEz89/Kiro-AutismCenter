using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IVideoAccessLogRepository
{
    Task AddAsync(VideoAccessLog log);
    Task<IEnumerable<VideoAccessLog>> GetUserAccessLogsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<VideoAccessLog>> GetModuleAccessLogsAsync(Guid moduleId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> CountFailedAccessAttemptsAsync(Guid userId, TimeSpan timeWindow);
}