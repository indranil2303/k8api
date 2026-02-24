using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace services.health;

public class RedisPingHealthCheck : IHealthCheck
{
    private readonly string _configuration;
    public RedisPingHealthCheck(string configuration) => _configuration = configuration;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var _cachedb = ConnectionMultiplexer.Connect(_configuration).GetDatabase();
            var ping = _cachedb.Ping();
            return Task.FromResult(HealthCheckResult.Healthy($"Redis ping successful: {ping.TotalMilliseconds} ms"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Redis ping failed: {ex.Message}"));
        }
    }
}