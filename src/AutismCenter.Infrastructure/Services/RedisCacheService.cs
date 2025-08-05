using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using AutismCenter.Application.Common.Interfaces;
using StackExchange.Redis;

namespace AutismCenter.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedValue))
                return null;

            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.SetAbsoluteExpiration(expiry.Value);
            }
            else
            {
                // Default expiry of 1 hour
                options.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            }

            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            
            var keys = server.Keys(pattern: pattern);
            var keyArray = keys.ToArray();
            
            if (keyArray.Length > 0)
            {
                await database.KeyDeleteAsync(keyArray);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached values by pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            return !string.IsNullOrEmpty(cachedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if cached value exists for key: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        
        if (cachedValue != null)
            return cachedValue;

        var item = await getItem();
        await SetAsync(key, item, expiry, cancellationToken);
        
        return item;
    }
}

public static class CacheKeys
{
    // Product caching
    public const string ProductById = "product:id:{0}";
    public const string ProductsByCategoryPattern = "products:category:{0}:page:{1}";
    public const string ProductsActivePattern = "products:active:*";
    
    // Course caching
    public const string CourseById = "course:id:{0}";
    public const string CoursesActivePattern = "courses:active:*";
    public const string CourseModules = "course:modules:{0}";
    
    // User caching
    public const string UserById = "user:id:{0}";
    public const string UserByEmail = "user:email:{0}";
    
    // Appointment caching
    public const string DoctorAvailability = "doctor:availability:{0}:{1}"; // doctorId:date
    public const string AppointmentsByUser = "appointments:user:{0}";
    
    // Order caching
    public const string OrderById = "order:id:{0}";
    public const string OrdersByUser = "orders:user:{0}";
    
    // Localization caching
    public const string LocalizedContent = "localized:content:{0}:{1}"; // key:language
    public const string EmailTemplate = "email:template:{0}:{1}"; // key:language
    
    // Statistics and analytics
    public const string DashboardStats = "dashboard:stats:{0}"; // date
    public const string ProductStats = "product:stats:{0}"; // productId
}