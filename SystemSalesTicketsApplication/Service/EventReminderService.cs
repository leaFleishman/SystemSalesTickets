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
                return 0;

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
                    _logger.LogError(
                        ex,
                        "Failed to send reminder to user {UserId} for event {EventId}",
                        group.Key.UserId,
                        group.Key.EventId);
                }
            }

            if (sentCount > 0)
                _logger.LogInformation("Event reminders sent: {Count}", sentCount);

            return sentCount;
        }

        private async Task MarkAsHandled(List<Order> orders, CancellationToken cancellationToken)
        {
            var timestamp = DateTime.UtcNow;

            foreach (var order in orders)
                order.ReminderSentAt = timestamp;

            await _orderRepository.Save(cancellationToken);
        }

        private TimeZoneInfo ResolveTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(_settings.TimeZoneId);
            }
            catch (Exception ex) when (
                ex is TimeZoneNotFoundException ||
                ex is InvalidTimeZoneException)
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

            var name = string.IsNullOrWhiteSpace(user.UserName)
                ? "לקוח/ה יקר/ה"
                : user.UserName;

            var safeName = WebUtility.HtmlEncode(name);
            var safeEventName = WebUtility.HtmlEncode(ev.Name);

            var subject = $"תזכורת לאירוע: {ev.Name}";

            var seatsHtml = seats.Count > 0
                ? $"""
                    <div style="
                        margin-top:18px;
                        padding:16px;
                        background:#f8fafc;
                        border:1px solid #e5e7eb;
                        border-radius:10px;
                    ">
                        <div style="
                            color:#6b7280;
                            font-size:13px;
                            margin-bottom:6px;
                        ">
                            המקומות שהוזמנו
                        </div>
                        <div style="
                            color:#111827;
                            font-size:16px;
                            font-weight:700;
                        ">
                            {WebUtility.HtmlEncode(string.Join("  •  ", seats))}
                        </div>
                    </div>
                    """
                : string.Empty;

            var html = $"""
                <!DOCTYPE html>
                <html lang="he" dir="rtl">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                </head>

                <body style="
                    margin:0;
                    padding:30px 15px;
                    background:#f5f7fb;
                    font-family:Arial,Helvetica,sans-serif;
                    direction:rtl;
                    color:#1f2937;
                ">

                    <div style="
                        max-width:600px;
                        margin:0 auto;
                        background:#ffffff;
                        border-radius:16px;
                        overflow:hidden;
                        box-shadow:0 5px 22px rgba(0,0,0,0.09);
                    ">

                        <!-- Header -->
                        <div style="
                            background:#1f2937;
                            color:#ffffff;
                            text-align:center;
                            padding:30px 22px;
                        ">
                            <div style="
                                width:54px;
                                height:54px;
                                line-height:54px;
                                margin:0 auto 14px;
                                border-radius:50%;
                                background:#f59e0b;
                                font-size:28px;
                                font-weight:bold;
                            ">
                                !
                            </div>

                            <h1 style="
                                margin:0;
                                font-size:23px;
                                font-weight:700;
                            ">
                                האירוע שלך מתקרב
                            </h1>

                            <p style="
                                margin:9px 0 0;
                                color:#d1d5db;
                                font-size:14px;
                            ">
                                תזכורת להזמנה שלך
                            </p>
                        </div>

                        <!-- Content -->
                        <div style="
                            padding:26px 24px 24px;
                        ">

                            <p style="
                                margin:0 0 20px;
                                font-size:16px;
                                color:#1f2937;
                            ">
                                שלום {safeName},
                            </p>

                            <p style="
                                margin:0 0 20px;
                                font-size:15px;
                                line-height:1.7;
                                color:#4b5563;
                            ">
                                רק תזכורת קטנה — האירוע
                                <strong style="color:#111827;">
                                    {safeEventName}
                                </strong>
                                מתקיים בקרוב.
                            </p>

                            <!-- Event details -->
                            <div style="
                                border:1px solid #e5e7eb;
                                border-radius:12px;
                                overflow:hidden;
                            ">

                                <div style="
                                    padding:16px;
                                    background:#fafafa;
                                    border-bottom:1px solid #e5e7eb;
                                ">
                                    <div style="color:#6b7280;font-size:13px;margin-bottom:5px;">
                                        האירוע
                                    </div>
                                    <div style="color:#111827;font-size:17px;font-weight:700;">
                                        {safeEventName}
                                    </div>
                                </div>

                                <div style="
                                    display:block;
                                    padding:15px 16px;
                                    border-bottom:1px solid #e5e7eb;
                                ">
                                    <span style="color:#6b7280;font-size:13px;">
                                        תאריך
                                    </span>
                                    <strong style="float:left;color:#111827;">
                                        {dateText}
                                    </strong>
                                </div>

                                <div style="
                                    display:block;
                                    padding:15px 16px;
                                ">
                                    <span style="color:#6b7280;font-size:13px;">
                                        שעה
                                    </span>
                                    <strong style="float:left;color:#111827;">
                                        {timeText}
                                    </strong>
                                </div>

                            </div>

                            {seatsHtml}

                            <!-- Reminder note -->
                            <div style="
                                margin-top:22px;
                                padding:16px 18px;
                                background:#fffbeb;
                                border:1px solid #fde68a;
                                border-radius:10px;
                                color:#92400e;
                                font-size:14px;
                                line-height:1.6;
                            ">
                                <strong>כדאי להגיע כמה דקות לפני תחילת האירוע.</strong>
                                <br>
                                מחכים לראות אותך!
                            </div>

                            <p style="
                                margin:25px 0 0;
                                text-align:center;
                                color:#9ca3af;
                                font-size:13px;
                            ">
                                תודה שבחרת ב-SystemSalesTickets
                            </p>

                        </div>
                    </div>
                </body>
                </html>
                """;

            var text = new StringBuilder();
            text.AppendLine($"שלום {name},");
            text.AppendLine();
            text.AppendLine($"האירוע "{ev.Name}" מתקיים בקרוב.");
            text.AppendLine($"תאריך: {dateText}");
            text.AppendLine($"שעה: {timeText}");

            if (seats.Count > 0)
                text.AppendLine($"המקומות שהזמנת: {string.Join(" | ", seats)}");

            text.AppendLine();
            text.AppendLine("כדאי להגיע כמה דקות לפני תחילת האירוע.");
            text.AppendLine("מחכים לראות אותך!");
            text.AppendLine();
            text.AppendLine("תודה שבחרת ב-SystemSalesTickets");

            return (subject, html, text.ToString());
        }
    }
}