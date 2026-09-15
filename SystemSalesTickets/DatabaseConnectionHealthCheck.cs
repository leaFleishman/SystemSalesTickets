using Microsoft.Extensions.Diagnostics.HealthChecks;
using SystemSalesTickets.Data;

namespace SystemSalesTickets;

public class DatabaseConnectionHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseConnectionHealthCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<DataContext>();

            await dbContext.Database.OpenConnectionAsync(cancellationToken);

            return HealthCheckResult.Healthy(
                "Database connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection exception",
                ex);
        }
    }
}