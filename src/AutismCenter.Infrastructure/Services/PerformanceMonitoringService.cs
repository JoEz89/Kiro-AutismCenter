using Microsoft.Extensions.Logging;
using AutismCenter.Application.Common.Services;

namespace AutismCenter.Infrastructure.Services;

public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly Dictionary<string, List<long>> _requestDurations = new();
    private readonly Dictionary<string, List<long>> _queryDurations = new();
    private readonly Dictionary<string, int> _cacheHits = new();
    private readonly Dictionary<string, int> _cacheMisses = new();
    private readonly object _lock = new();

    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger;
    }

    public void TrackRequestDuration(string endpoint, long durationMs)
    {
        lock (_lock)
        {
            if (!_requestDurations.ContainsKey(endpoint))
                _requestDurations[endpoint] = new List<long>();
            
            _requestDurations[endpoint].Add(durationMs);
            
            if (durationMs > 1000) // Log slow requests
            {
                _logger.LogWarning("Slow request detected: {Endpoint} took {Duration}ms", endpoint, durationMs);
            }
        }
    }

    public void TrackDatabaseQuery(string query, long durationMs)
    {
        lock (_lock)
        {
            if (!_queryDurations.ContainsKey(query))
                _queryDurations[query] = new List<long>();
            
            _queryDurations[query].Add(durationMs);
            
            if (durationMs > 500) // Log slow queries
            {
                _logger.LogWarning("Slow database query detected: {Query} took {Duration}ms", query, durationMs);
            }
        }
    }

    public void TrackCacheHit(string key)
    {
        lock (_lock)
        {
            _cacheHits[key] = _cacheHits.GetValueOrDefault(key, 0) + 1;
        }
    }

    public void TrackCacheMiss(string key)
    {
        lock (_lock)
        {
            _cacheMisses[key] = _cacheMisses.GetValueOrDefault(key, 0) + 1;
        }
    }

    public Task<PerformanceMetrics> GetMetricsAsync(DateTime from, DateTime to)
    {
        lock (_lock)
        {
            var metrics = new PerformanceMetrics();
            
            // Calculate average response time
            var allDurations = _requestDurations.Values.SelectMany(x => x).ToList();
            if (allDurations.Any())
            {
                metrics.AverageResponseTime = allDurations.Average();
                metrics.TotalRequests = allDurations.Count;
                metrics.SlowRequests = allDurations.Count(x => x > 1000);
            }
            
            // Calculate average database query time
            var allQueryDurations = _queryDurations.Values.SelectMany(x => x).ToList();
            if (allQueryDurations.Any())
            {
                metrics.AverageDatabaseQueryTime = allQueryDurations.Average();
            }
            
            // Calculate cache hit ratio
            var totalHits = _cacheHits.Values.Sum();
            var totalMisses = _cacheMisses.Values.Sum();
            var totalCacheRequests = totalHits + totalMisses;
            
            if (totalCacheRequests > 0)
            {
                metrics.CacheHitRatio = (double)totalHits / totalCacheRequests * 100;
            }
            
            // Calculate endpoint performance
            foreach (var endpoint in _requestDurations.Keys)
            {
                if (_requestDurations[endpoint].Any())
                {
                    metrics.EndpointPerformance[endpoint] = _requestDurations[endpoint].Average();
                }
            }
            
            return Task.FromResult(metrics);
        }
    }
}