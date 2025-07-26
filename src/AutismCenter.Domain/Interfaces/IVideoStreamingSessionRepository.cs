using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface IVideoStreamingSessionRepository
{
    Task<VideoStreamingSession?> GetActiveSessionAsync(Guid userId, Guid moduleId);
    Task<VideoStreamingSession?> GetBySessionIdAsync(string sessionId);
    Task<IEnumerable<VideoStreamingSession>> GetActiveSessionsForUserAsync(Guid userId);
    Task AddAsync(VideoStreamingSession session);
    Task UpdateAsync(VideoStreamingSession session);
    Task<int> CountActiveSessionsAsync(Guid userId);
    Task EndExpiredSessionsAsync(int maxDurationMinutes = 120);
}