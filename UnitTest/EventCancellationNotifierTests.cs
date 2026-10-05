using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Settings;
using SystemSalesTickets.Service.Service;

namespace UnitTest
{
    public class EventCancellationNotifierTests
    {
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly EventCancellationNotifier _notifier;

        public EventCancellationNotifierTests()
        {
            _notifier = new EventCancellationNotifier(
                _emailServiceMock.Object,
                Options.Create(new ReminderSettings { TimeZoneId = "UTC" }),
                new Mock<ILogger<EventCancellationNotifier>>().Object);
        }

        private static Event CancelledEvent() => new Event
        {
            Id = 1,
            Name = "Concert A",
            Date = new DateTime(2026, 12, 24, 18, 30, 0, DateTimeKind.Utc),
            IsCancelled = true
        };

        private static Order CreateOrder(int id, int userId, string? email, int seatId) => new Order
        {
            Id = id,
            UserId = userId,
            EventId = 1,
            EventName = "Concert A",
            SeatId = seatId,
            User = new User { Id = userId, UserName = "User" + userId, Email = email! },
            Seat = new Seat { Id = seatId, Row = 1, Line = seatId }
        };

        private void VerifyEmailsSent(Times times)
        {
            _emailServiceMock.Verify(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), times);
        }

        [Fact]
        public async Task Notify_SendsOneEmailPerCustomer()
        {
            var orders = new List<Order>
            {
                CreateOrder(1, 2, "moshe@example.com", 1),
                CreateOrder(2, 2, "moshe@example.com", 2), // same customer, two seats
                CreateOrder(3, 3, "dana@example.com", 3)
            };

            var sent = await _notifier.NotifyAsync(CancelledEvent(), orders, "Artist is ill");

            Assert.Equal(2, sent);
            VerifyEmailsSent(Times.Exactly(2));
        }

        [Fact]
        public async Task Notify_EmailContainsEventNameAndReason()
        {
            var orders = new List<Order> { CreateOrder(1, 2, "moshe@example.com", 1) };

            await _notifier.NotifyAsync(CancelledEvent(), orders, "Artist is ill");

            _emailServiceMock.Verify(e => e.SendAsync(
                "moshe@example.com",
                "User2",
                It.Is<string>(subject => subject.Contains("Concert A")),
                It.Is<string>(html => html.Contains("Concert A") && html.Contains("Artist is ill") && html.Contains("24/12/2026")),
                It.Is<string?>(text => text != null && text.Contains("Artist is ill")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Notify_WithoutReason_OmitsReasonLine()
        {
            var orders = new List<Order> { CreateOrder(1, 2, "moshe@example.com", 1) };

            await _notifier.NotifyAsync(CancelledEvent(), orders, null);

            _emailServiceMock.Verify(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.Is<string>(html => !html.Contains("סיבת הביטול")),
                It.Is<string?>(text => text != null && !text.Contains("סיבת הביטול")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-an-email")]
        public async Task Notify_SkipsCustomersWithoutValidEmail(string? email)
        {
            var orders = new List<Order> { CreateOrder(1, 2, email, 1) };

            var sent = await _notifier.NotifyAsync(CancelledEvent(), orders, "reason");

            Assert.Equal(0, sent);
            VerifyEmailsSent(Times.Never());
        }

        [Fact]
        public async Task Notify_WhenOneSendFails_ContinuesWithTheOthers()
        {
            var orders = new List<Order>
            {
                CreateOrder(1, 2, "broken@example.com", 1),
                CreateOrder(2, 3, "dana@example.com", 2)
            };

            _emailServiceMock
                .Setup(e => e.SendAsync(
                    "broken@example.com", It.IsAny<string?>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("SMTP error"));

            var sent = await _notifier.NotifyAsync(CancelledEvent(), orders, "reason");

            Assert.Equal(1, sent);
            VerifyEmailsSent(Times.Exactly(2));
        }
    }
}
