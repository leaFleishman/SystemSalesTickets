using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Core.Settings;

namespace SystemSalesTickets.Service.Service
{
    public class EventReminderService : IEventReminderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailService _emailService;
        private readonly ReminderSettings _settings;
        private readonly ILogger<EventReminderService> _logger;

        public EventReminderService(
            IOrderRepository orderRepository,
            IEmailService emailService,
            IOptions<ReminderSettings> settings,
            ILogger<EventReminderService> logger)
        {
            _orderRepository = orderRepository;
            _emailService = emailService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<int> SendDueRemindersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var windowEnd = now.AddHours(_settings.HoursBeforeEvent);

            var pendingOrders = await _orderRepository.GetOrdersPendingReminder(
                now,
                windowEnd,
                cancellationToken);

            if (pendingOrders.Count == 0)
            {
                return 0;
            }

            var timeZone = ResolveTimeZone();
            var sentCount = 0;

            // One email per customer per event, even if they bought several seats.
            foreach (var group in pendingOrders.GroupBy(o => new { o.UserId, o.EventId }))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var orders = group.ToList();
                var user = orders[0].User;
                var ev = orders[0].Event;
                var address = user?.Email?.Trim();

                if (user == null || ev == null || !MailAddress.TryCreate(address, out _))
                {
                    // Nothing we can ever deliver to: mark as handled so we don't retry forever.
                    _logger.LogWarning(
                        "Skipping reminder for user {UserId}, event {EventId}: missing or invalid email '{Email}'",
                        group.Key.UserId,
                        group.Key.EventId,
                        address);

                    await MarkAsHandled(orders, cancellationToken);
                    continue;
                }

                try
                {
                    var (subject, html, text) = BuildMessage(user, ev, orders, timeZone);

                    await _emailService.SendAsync(
                        address!,
                        user.UserName,
                        subject,
                        html,
                        text,
                        cancellationToken);

                    await MarkAsHandled(orders, cancellationToken);
                    sentCount++;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Not marked, so the next run retries this customer.
                    _logger.LogError(
                        ex,
                        "Failed to send reminder to user {UserId} for event {EventId}",
                        group.Key.UserId,
                        group.Key.EventId);
                }
            }

            if (sentCount > 0)
            {
                _logger.LogInformation("Event reminders sent: {Count}", sentCount);
            }

            return sentCount;
        }

        private async Task MarkAsHandled(List<Order> orders, CancellationToken cancellationToken)
        {
            var timestamp = DateTime.UtcNow;
            foreach (var order in orders)
            {
                order.ReminderSentAt = timestamp;
            }

            await _orderRepository.Save(cancellationToken);
        }

        private TimeZoneInfo ResolveTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(_settings.TimeZoneId);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException)
            {
                _logger.LogWarning(
                    "Time zone '{TimeZoneId}' not found, using UTC in reminder emails",
                    _settings.TimeZoneId);
                return TimeZoneInfo.Utc;
            }
        }

        private static (string Subject, string Html, string Text) BuildMessage(
            User user,
            Event ev,
            List<Order> orders,
            TimeZoneInfo timeZone)
        {
            var eventUtc = DateTime.SpecifyKind(ev.Date, DateTimeKind.Utc);
            var local = TimeZoneInfo.ConvertTimeFromUtc(eventUtc, timeZone);
            var dateText = local.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            var timeText = local.ToString("HH:mm", CultureInfo.InvariantCulture);

            var seats = orders
                .Where(o => o.Seat != null)
                .OrderBy(o => o.Seat.Row)
                .ThenBy(o => o.Seat.Line)
                .Select(o => $"שורה {o.Seat.Row}, מושב {o.Seat.Line}")
                .ToList();

            var name = string.IsNullOrWhiteSpace(user.UserName) ? "לקוח/ה יקר/ה" : user.UserName;
            var subject = $"תזכורת לאירוע: {ev.Name}";

            var html = new StringBuilder();
            html.Append("<div dir=\"rtl\" style=\"font-family:Arial,Helvetica,sans-serif;font-size:16px;color:#222;line-height:1.6\">");
            html.Append($"<p>שלום {WebUtility.HtmlEncode(name)},</p>");
            html.Append($"<p>זוהי תזכורת לכך שהאירוע <strong>{WebUtility.HtmlEncode(ev.Name)}</strong> מתקיים בקרוב.</p>");
            html.Append("<ul>");
            html.Append($"<li>תאריך: {dateText}</li>");
            html.Append($"<li>שעה: {timeText}</li>");
            if (seats.Count > 0)
            {
                html.Append($"<li>המקומות שהזמנת: {WebUtility.HtmlEncode(string.Join(" | ", seats))}</li>");
            }
            html.Append("</ul>");
            html.Append("<p>מצפים לראות אותך!</p>");
            html.Append("</div>");

            var text = new StringBuilder();
            text.AppendLine($"שלום {name},");
            text.AppendLine();
            text.AppendLine($"זוהי תזכורת לכך שהאירוע \"{ev.Name}\" מתקיים בקרוב.");
            text.AppendLine($"תאריך: {dateText}");
            text.AppendLine($"שעה: {timeText}");
            if (seats.Count > 0)
            {
                text.AppendLine($"המקומות שהזמנת: {string.Join(" | ", seats)}");
            }
            text.AppendLine();
            text.AppendLine("מצפים לראות אותך!");

            return (subject, html.ToString(), text.ToString());
        }
    }
}
