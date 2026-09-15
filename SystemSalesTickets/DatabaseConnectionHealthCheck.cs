using Microsoft.Extensions.Diagnostics.HealthChecks;
using SystemSalesTickets.Data;

namespace SystemSalesTickets;

public class DatabaseConnectionHealthCheck : IHealthCheck
{
    private readonly DataContext _dbContext;

    public DatabaseConnectionHealthCheck(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("Database connection successful")
                : HealthCheckResult.Unhealthy("Database connection failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection exception",
                ex);
        }
    }
}