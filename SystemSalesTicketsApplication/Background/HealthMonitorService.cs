using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Data;

namespace SystemSalesTickets.Service.Background;

public class HealthMonitorService : BackgroundService
{
    private readonly DataContext _dbContext;
    private readonly ILogger<HealthMonitorService> _logger;

    public HealthMonitorService(
        DataContext dbContext,
        ILogger<HealthMonitorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync(
                    stoppingToken);

                if (canConnect)
                {
                    _logger.LogInformation(
                        "Health Check: Database connection is Healthy");
                }
                else
                {
                    _logger.LogError(
                        "Health Check: Database connection returned false");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Health Check failed: Database connection error");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }
    }
}