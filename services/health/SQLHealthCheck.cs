using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;

namespace services.health;

public class SQLHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    public SQLHealthCheck(string connectionString) => _connectionString = connectionString;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            await conn.CloseAsync();
            return HealthCheckResult.Healthy("SQL connection OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"SQL connection failed: {ex.Message}");
        }
    }
}
