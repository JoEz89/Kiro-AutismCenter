namespace AutismCenter.Application.Common.Services;

public interface IPerformanceMonitoringService
{
    void TrackRequestDuration(string endpoint, long durationMs);
    void TrackDatabaseQuery(string query, long durationMs);
    void TrackCacheHit(string key);
    void TrackCacheMiss(string key);
    Task<PerformanceMetrics> GetMetricsAsync(DateTime from, DateTime to);
}

public class PerformanceMetrics
{
    public double AverageResponseTime { get; set; }
    public double AverageDatabaseQueryTime { get; set; }
    public double CacheHitRatio { get; set; }
    public int TotalRequests { get; set; }
    public int SlowRequests { get; set; }
    public Dictionary<string, double> EndpointPerformance { get; set; } = new();
}