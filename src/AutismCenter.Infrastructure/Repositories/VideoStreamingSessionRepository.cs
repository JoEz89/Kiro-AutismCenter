using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutismCenter.Infrastructure.Repositories;

public class VideoStreamingSessionRepository : IVideoStreamingSessionRepository
{
    private readonly ApplicationDbContext _context;

    public VideoStreamingSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VideoStreamingSession?> GetActiveSessionAsync(Guid userId, Guid moduleId)
    {
        return await _context.VideoStreamingSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ModuleId == moduleId && s.IsActive);
    }

    public async Task<VideoStreamingSession?> GetBySessionIdAsync(string sessionId)
    {
        return await _context.VideoStreamingSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<IEnumerable<VideoStreamingSession>> GetActiveSessionsForUserAsync(Guid userId)
    {
        return await _context.VideoStreamingSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(VideoStreamingSession session)
    {
        await _context.VideoStreamingSessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(VideoStreamingSession session)
    {
        _context.VideoStreamingSessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountActiveSessionsAsync(Guid userId)
    {
        return await _context.VideoStreamingSessions
            .CountAsync(s => s.UserId == userId && s.IsActive);
    }

    public async Task EndExpiredSessionsAsync(int maxDurationMinutes = 120)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-maxDurationMinutes);
        
        var expiredSessions = await _context.VideoStreamingSessions
            .Where(s => s.IsActive && s.StartTime < cutoffTime)
            .ToListAsync();

        foreach (var session in expiredSessions)
        {
            session.EndSession();
        }

        if (expiredSessions.Any())
        {
            await _context.SaveChangesAsync();
        }
    }
}