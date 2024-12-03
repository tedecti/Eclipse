using Eclipse.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Eclipse.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _defaultCacheOptions;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _defaultCacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));
    }

    public T? Get<T>(string key)
    {
        return _memoryCache.TryGetValue(key, out T? value) ? value : default;
    }

    public void Set<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        var options = expirationTime.HasValue
            ? new MemoryCacheEntryOptions().SetSlidingExpiration(expirationTime.Value)
            : _defaultCacheOptions;

        _memoryCache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }
}