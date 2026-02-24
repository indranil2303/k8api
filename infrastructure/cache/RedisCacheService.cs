using StackExchange.Redis;
using System.Text.Json;

namespace infrastructure.cache;

public class RedisCacheService<TEntity> : ICacheService<TEntity>, ICacheVersionService<TEntity>
    where TEntity : class
{
    private readonly IDatabase _cachedb;
    private static JsonSerializerOptions JsonOptions => new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _cachedb = connectionMultiplexer.GetDatabase();
    }

    public async Task<TEntity?> GetAsync(string key)
    {
        var json = await _cachedb.StringGetAsync(key);

        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<TEntity>(json!, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan ttl)
    {
        await _cachedb.StringSetAsync(key, value, ttl);
    }

    public async Task RemoveAsync(string key)
    {
        await _cachedb.KeyDeleteAsync(key);
    }

    public async Task RemoveAsyncPrefix(string prefix)
    {
        var server = _cachedb.Multiplexer.GetServer(_cachedb.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{prefix}*").ToArray();
        if (keys.Length > 0)
        {
            await _cachedb.KeyDeleteAsync(keys);
        }
    }

    public async Task<int> GetVersionAsync(string entity)
    {
        var v = await _cachedb.StringGetAsync($"{entity}:version");
        return v.HasValue ? (int)v : 1;
    }

    public async Task IncrementVersionAsync(string entity)
    {
        await _cachedb.StringIncrementAsync($"{entity}:version");
    }
}