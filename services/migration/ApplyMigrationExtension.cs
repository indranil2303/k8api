using infrastructure.persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace services.migration;

public static class ApplyMigrationExtension
{
    public static async Task<WebApplication> ApplyMigrationsAsync<TDbContext>(
        this WebApplication app,
        int maxRetries = 10,
        int delaySeconds = 5)
        where TDbContext : LocaldbContext
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<TDbContext>>();
        var db = services.GetRequiredService<TDbContext>();

        var retries = maxRetries;

        while (retries > 0)
        {
            try
            {
                logger.LogInformation("Checking for pending EF migrations...");

                var pending = await db.Database.GetPendingMigrationsAsync();

                if (pending.Any())
                {
                    logger.LogInformation(
                        "Applying {Count} migrations: {Migrations}",
                        pending.Count(),
                        string.Join(", ", pending));

                    await db.Database.MigrateAsync();

                    logger.LogInformation("EF migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("No pending EF migrations.");
                }
                break;
            }
            catch (SqlException ex)
            {
                retries--;
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

                logger.LogWarning(
                    ex,
                    "Database not ready. Retrying in {Delay}s ({Retries} left)",
                    delaySeconds,
                    retries);

                if (retries == 0) throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error during EF migration");
                throw;
            }
        }

        return app; // SUCCESS — run once only
    }
}