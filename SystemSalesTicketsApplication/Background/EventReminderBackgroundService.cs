using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Settings;

namespace SystemSalesTickets.Service.Background;

/// <summary>
/// Periodically sends reminder emails to customers whose event starts within the configured window.
/// </summary>
public class EventReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ReminderSettings _reminderSettings;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EventReminderBackgroundService> _logger;

    public EventReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<ReminderSettings> reminderSettings,
        IOptions<EmailSettings> emailSettings,
        ILogger<EventReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _reminderSettings = reminderSettings.Value;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_reminderSettings.Enabled)
        {
            _logger.LogInformation("Event reminder job is disabled (Reminder:Enabled=false)");
            return;
        }

        if (!_emailSettings.IsConfigured)
        {
            _logger.LogWarning(
     "Event reminder job not started: email is not configured (Email:ApiKey / Email:FromAddress)");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Max(1, _reminderSettings.CheckIntervalMinutes));

        _logger.LogInformation(
            "Event reminder job started: reminding {Hours}h before events, checking every {Interval}",
            _reminderSettings.HoursBeforeEvent,
            interval);

        try
        {
            using var timer = new PeriodicTimer(interval);

            do
            {
                await RunOnce(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private async Task RunOnce(CancellationToken stoppingToken)
    {
        try
        {
            // Repositories/DbContext are scoped, so each run gets its own scope.
            using var scope = _scopeFactory.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IEventReminderService>();
            await reminderService.SendDueRemindersAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Never let one failed run kill the loop (or the host).
            _logger.LogError(ex, "Event reminder run failed");
        }
    }
}
