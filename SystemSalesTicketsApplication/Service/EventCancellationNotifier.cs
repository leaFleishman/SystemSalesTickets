using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Settings;

namespace SystemSalesTickets.Service.Service
{
    public class EventCancellationNotifier : IEventCancellationNotifier
    {
        private readonly IEmailService _emailService;
        private readonly ReminderSettings _settings;
        private readonly ILogger<EventCancellationNotifier> _logger;

        public EventCancellationNotifier(
            IEmailService emailService,
            IOptions<ReminderSettings> settings,
            ILogger<EventCancellationNotifier> logger)
        {
            _emailService = emailService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<int> NotifyAsync(
            Event cancelledEvent,
            IReadOnlyCollection<Order> orders,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            var timeZone = ResolveTimeZone();
            var sent = 0;

            // One email per customer, even if they bought several seats.
            foreach (var group in orders.GroupBy(o => o.UserId))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var user = group.First().User;
                var address = user?.Email?.Trim();

                if (user == null || !MailAddress.TryCreate(address, out _))
                {
                    _logger.LogWarning(
                        "Skipping cancellation email for user {UserId}, event {EventId}: missing or invalid email '{Email}'",
                        group.Key,
                        cancelledEvent.Id,
                        address);
                    continue;
                }

                try
                {
                    var (subject, html, text) = BuildMessage(user, cancelledEvent, group.ToList(), reason, timeZone);

                    await _emailService.SendAsync(
                        address!,
                        user.UserName,
                        subject,
                        html,
                        text,
                        cancellationToken);

                    sent++;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send cancellation email to user {UserId} for event {EventId}",
                        group.Key,
                        cancelledEvent.Id);
                }
            }

            return sent;
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
                    "Time zone '{TimeZoneId}' not found, using UTC in cancellation emails",
                    _settings.TimeZoneId);
                return TimeZoneInfo.Utc;
            }
        }

        private static (string Subject, string Html, string Text) BuildMessage(
            User user,
            Event ev,
            List<Order> orders,
            string? reason,
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
            var subject = $"האירוע בוטל: {ev.Name}";
            var hasReason = !string.IsNullOrWhiteSpace(reason);

            var html = new StringBuilder();
            html.Append("<div dir=\"rtl\" style=\"font-family:Arial,Helvetica,sans-serif;font-size:16px;color:#222;line-height:1.6\">");
            html.Append($"<p>שלום {WebUtility.HtmlEncode(name)},</p>");
            html.Append($"<p>לצערנו האירוע <strong>{WebUtility.HtmlEncode(ev.Name)}</strong> שהיה אמור להתקיים בתאריך {dateText} בשעה {timeText} בוטל.</p>");
            if (hasReason)
            {
                html.Append($"<p>סיבת הביטול: {WebUtility.HtmlEncode(reason!)}</p>");
            }
            if (seats.Count > 0)
            {
                html.Append($"<p>ההזמנות שבוטלו: {WebUtility.HtmlEncode(string.Join(" | ", seats))}</p>");
            }
            html.Append("<p>אנו מתנצלים על אי הנוחות.</p>");
            html.Append("</div>");

            var text = new StringBuilder();
            text.AppendLine($"שלום {name},");
            text.AppendLine();
            text.AppendLine($"לצערנו האירוע \"{ev.Name}\" שהיה אמור להתקיים בתאריך {dateText} בשעה {timeText} בוטל.");
            if (hasReason)
            {
                text.AppendLine($"סיבת הביטול: {reason}");
            }
            if (seats.Count > 0)
            {
                text.AppendLine($"ההזמנות שבוטלו: {string.Join(" | ", seats)}");
            }
            text.AppendLine();
            text.AppendLine("אנו מתנצלים על אי הנוחות.");

            return (subject, html.ToString(), text.ToString());
        }
    }
}
