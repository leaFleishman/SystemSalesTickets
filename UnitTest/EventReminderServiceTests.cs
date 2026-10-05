using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Core.Settings;
using SystemSalesTickets.Service.Service;

namespace UnitTest
{
    public class EventReminderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly EventReminderService _service;

        public EventReminderServiceTests()
        {
            _service = new EventReminderService(
                _orderRepositoryMock.Object,
                _emailServiceMock.Object,
                Options.Create(new ReminderSettings { TimeZoneId = "UTC" }),
                new Mock<ILogger<EventReminderService>>().Object);
        }

        private static Order CreateOrder(int id, int userId, string email, int seatId)
        {
            return new Order
            {
                Id = id,
                UserId = userId,
                EventId = 1,
                EventName = "Concert A",
                SeatId = seatId,
                User = new User { Id = userId, UserName = "Moshe", Email = email },
                Event = new Event { Id = 1, Name = "Concert A", Date = DateTime.UtcNow.AddHours(23) },
                Seat = new Seat { Id = seatId, Row = 1, Line = seatId }
            };
        }

        private void SetupPending(params Order[] orders)
        {
            _orderRepositoryMock
                .Setup(r => r.GetOrdersPendingReminder(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(orders.ToList());
        }

        private void VerifyEmailsSent(Times times)
        {
            _emailServiceMock.Verify(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), times);
        }

        [Fact]
        public async Task SendDueReminders_WhenNothingPending_SendsNothing()
        {
            SetupPending();

            var sent = await _service.SendDueRemindersAsync();

            Assert.Equal(0, sent);
            VerifyEmailsSent(Times.Never());
        }

        [Fact]
        public async Task SendDueReminders_SendsOneEmailPerCustomerAndMarksAllOrders()
        {
            var first = CreateOrder(1, 2, "moshe@example.com", 1);
            var second = CreateOrder(2, 2, "moshe@example.com", 2); // same customer, same event
            var other = CreateOrder(3, 3, "dana@example.com", 3);
            SetupPending(first, second, other);

            var sent = await _service.SendDueRemindersAsync();

            Assert.Equal(2, sent);
            VerifyEmailsSent(Times.Exactly(2));
            Assert.NotNull(first.ReminderSentAt);
            Assert.NotNull(second.ReminderSentAt);
            Assert.NotNull(other.ReminderSentAt);
            _orderRepositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task SendDueReminders_WhenEmailInvalid_SkipsSendingAndMarksHandled()
        {
            var order = CreateOrder(1, 1, "15000", 1);
            SetupPending(order);

            var sent = await _service.SendDueRemindersAsync();

            Assert.Equal(0, sent);
            VerifyEmailsSent(Times.Never());
            Assert.NotNull(order.ReminderSentAt);
        }

        [Fact]
        public async Task SendDueReminders_WhenSendingFails_DoesNotMarkOrderSoItIsRetried()
        {
            var order = CreateOrder(1, 2, "moshe@example.com", 1);
            SetupPending(order);
            _emailServiceMock
                .Setup(e => e.SendAsync(
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("SMTP down"));

            var sent = await _service.SendDueRemindersAsync();

            Assert.Equal(0, sent);
            Assert.Null(order.ReminderSentAt);
            _orderRepositoryMock.Verify(r => r.Save(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
