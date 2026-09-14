using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SystemSalesTickets.Service.Background;

public class HealthMonitorService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthMonitorService> _logger;

    public HealthMonitorService(
        HealthCheckService healthCheckService,
        ILogger<HealthMonitorService> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _healthCheckService.CheckHealthAsync(
                    stoppingToken);

                if (result.Status == HealthStatus.Healthy)
                {
                    _logger.LogInformation(
                        "Health Check: System is Healthy");
                }
                else
                {
                    _logger.LogWarning(
                        "Health Check: System status is {Status}",
                        result.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Health Check failed");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }
}