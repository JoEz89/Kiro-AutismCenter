using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutismCenter.Infrastructure.Repositories;

public class VideoAccessLogRepository : IVideoAccessLogRepository
{
    private readonly ApplicationDbContext _context;

    public VideoAccessLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(VideoAccessLog log)
    {
        await _context.VideoAccessLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<VideoAccessLog>> GetUserAccessLogsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.VideoAccessLogs.Where(l => l.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(l => l.AccessTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.AccessTime <= toDate.Value);

        return await query
            .OrderByDescending(l => l.AccessTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<VideoAccessLog>> GetModuleAccessLogsAsync(Guid moduleId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.VideoAccessLogs.Where(l => l.ModuleId == moduleId);

        if (fromDate.HasValue)
            query = query.Where(l => l.AccessTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.AccessTime <= toDate.Value);

        return await query
            .OrderByDescending(l => l.AccessTime)
            .ToListAsync();
    }

    public async Task<int> CountFailedAccessAttemptsAsync(Guid userId, TimeSpan timeWindow)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
        
        return await _context.VideoAccessLogs
            .CountAsync(l => l.UserId == userId && 
                           !l.AccessGranted && 
                           l.AccessTime >= cutoffTime);
    }
}