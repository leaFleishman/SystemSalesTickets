using Microsoft.Extensions.Logging;
using System.Globalization;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Service.Service
{
    public sealed class OrderConfirmationEmailService
        : IOrderConfirmationEmailService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderConfirmationEmailService> _logger;

        public OrderConfirmationEmailService(
            IEmailService emailService,
            ILogger<OrderConfirmationEmailService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendAsync(
            Order order,
            string email,
            string? userName,
            CancellationToken cancellationToken = default)
        {
            var eventName = order.EventName ?? order.Event?.Name ?? "אירוע";
            var eventDate = order.Event?.Date;
            var price = order.Event?.Price;
            var seat = order.Seat;

            var dateText = eventDate.HasValue
                ? eventDate.Value.ToLocalTime().ToString(
                    "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture)
                : "לא צוין";

            var priceText = price.HasValue
                ? $"{price.Value:N2} ₪"
                : "לא צוין";

            var seatText = seat != null
                ? $"שורה {seat.Row} · טור {seat.Line}"
                : "לא צוין";

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
                ">
                    <div style="
                        max-width:600px;
                        margin:0 auto;
                        background:white;
                        border-radius:14px;
                        overflow:hidden;
                        box-shadow:0 4px 18px rgba(0,0,0,0.08);
                    ">

                        <div style="
                            background:#1f2937;
                            color:white;
                            text-align:center;
                            padding:28px 20px;
                        ">
                            <div style="
                                width:48px;
                                height:48px;
                                line-height:48px;
                                margin:0 auto 12px;
                                border-radius:50%;
                                background:#22c55e;
                                font-size:28px;
                                font-weight:bold;
                            ">✓</div>

                            <h1 style="
                                margin:0;
                                font-size:22px;
                                font-weight:700;
                            ">
                                ההזמנה אושרה
                            </h1>

                            <p style="
                                margin:8px 0 0;
                                opacity:.85;
                                font-size:14px;
                            ">
                                מספר הזמנה #{order.Id}
                            </p>
                        </div>

                        <div style="
                            padding:25px 24px;
                            color:#1f2937;
                        ">

                            <p style="
                                margin:0 0 22px;
                                font-size:16px;
                            ">
                                שלום {System.Net.WebUtility.HtmlEncode(userName ?? "")},
                            </p>

                            <p style="
                                margin:0 0 22px;
                                font-size:15px;
                                color:#4b5563;
                            ">
                                ההזמנה שלך בוצעה בהצלחה. הנה פרטי ההזמנה:
                            </p>

                            <div style="
                                border:1px solid #e5e7eb;
                                border-radius:10px;
                                overflow:hidden;
                            ">

                                <div style="
                                    padding:14px 16px;
                                    border-bottom:1px solid #e5e7eb;
                                ">
                                    <span style="color:#6b7280;">אירוע</span>
                                    <strong style="float:left;">
                                        {System.Net.WebUtility.HtmlEncode(eventName)}
                                    </strong>
                                </div>

                                <div style="
                                    padding:14px 16px;
                                    border-bottom:1px solid #e5e7eb;
                                ">
                                    <span style="color:#6b7280;">תאריך</span>
                                    <strong style="float:left;">
                                        {dateText}
                                    </strong>
                                </div>

                                <div style="
                                    padding:14px 16px;
                                    border-bottom:1px solid #e5e7eb;
                                ">
                                    <span style="color:#6b7280;">מושב</span>
                                    <strong style="float:left;">
                                        {seatText}
                                    </strong>
                                </div>

                                <div style="
                                    padding:14px 16px;
                                    border-bottom:1px solid #e5e7eb;
                                ">
                                    <span style="color:#6b7280;">מחיר</span>
                                    <strong style="float:left;">
                                        {priceText}
                                    </strong>
                                </div>

                                <div style="
                                    padding:14px 16px;
                                ">
                                    <span style="color:#6b7280;">מועד ההזמנה</span>
                                    <strong style="float:left;">
                                        {order.OrderDate.ToLocalTime():dd/MM/yyyy HH:mm}
                                    </strong>
                                </div>

                            </div>

                            <p style="
                                margin:25px 0 0;
                                text-align:center;
                                color:#6b7280;
                                font-size:13px;
                            ">
                                תודה שבחרת ב-SystemSalesTickets
                            </p>

                        </div>
                    </div>
                </body>
                </html>
                """;

            var text = $"""
                ההזמנה אושרה

                מספר הזמנה: #{order.Id}
                אירוע: {eventName}
                תאריך: {dateText}
                מושב: {seatText}
                מחיר: {priceText}
                מועד ההזמנה: {order.OrderDate.ToLocalTime():dd/MM/yyyy HH:mm}

                תודה שבחרת ב-SystemSalesTickets
                """;

            await _emailService.SendAsync(
                email,
                userName,
                $"אישור הזמנה #{order.Id} - {eventName}",
                html,
                text,
                cancellationToken);

            _logger.LogInformation(
                "Order confirmation email sent for OrderId {OrderId}",
                order.Id);
        }
    }
}