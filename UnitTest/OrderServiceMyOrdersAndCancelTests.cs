using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using SystemSalesTickets.Core;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;

namespace UnitTest
{
    public class OrderServiceMyOrdersAndCancelTests
    {
        private sealed class FixedTimeProvider : TimeProvider
        {
            private readonly DateTimeOffset _now;
            public FixedTimeProvider(DateTime utcNow) => _now = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
            public override DateTimeOffset GetUtcNow() => _now;
        }

        private static readonly DateTime Now = new(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc);

        private readonly Mock<IOrderRepository> _orders = new();
        private readonly Mock<IEventSeatRepository> _eventSeats = new();
        private readonly OrderService _service;

        public OrderServiceMyOrdersAndCancelTests()
        {
            _service = new OrderService(
                _orders.Object,
                new Mock<IMapper>().Object,
                new Mock<ILogger<OrderService>>().Object,
                _eventSeats.Object,
                new Mock<IUserRepository>().Object,
                new Mock<IOrderConfirmationEmailService>().Object,
                new FixedTimeProvider(Now));
        }

        private static Order MakeOrder(DateTime eventDate, int userId = 2, bool eventCancelled = false) => new()
        {
            Id = 10,
            UserId = userId,
            EventId = 1,
            SeatId = 5,
            EventName = "Concert A",
            OrderDate = Now.AddDays(-3),
            Event = new Event { Id = 1, Name = "Concert A", Date = eventDate, Price = 120, IsCancelled = eventCancelled },
            Seat = new Seat { Id = 5, Row = 3, Line = 7 }
        };

        private EventSeat SetupCancellable(Order order)
        {
            var eventSeat = new EventSeat { EventId = 1, SeatId = 5, IsAvailable = false };
            _orders.Setup(x => x.GetByIdForCancellation(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
            _eventSeats.Setup(x => x.GetByEventAndSeat(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(eventSeat);
            return eventSeat;
        }

        // ---------- GetMyOrders ----------

        [Fact]
        public async Task GetMyOrders_ReturnsEventNameDateTimePriceAndSeat()
        {
            var eventDate = Now.AddDays(5);
            _orders.Setup(x => x.GetOrdersByUser(2, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<Order> { MakeOrder(eventDate) });

            var result = await _service.GetMyOrders(2);

            var dto = Assert.Single(result);
            Assert.Equal(10, dto.OrderId);
            Assert.Equal("Concert A", dto.EventName);
            Assert.Equal(eventDate, dto.EventDate);
            Assert.Equal(120m, dto.Price);
            Assert.Equal(3, dto.Row);
            Assert.Equal(7, dto.Line);
            Assert.True(dto.CanCancel);
            Assert.Equal(eventDate.AddHours(-24), dto.CancellationDeadline);
        }

        [Fact]
        public async Task GetMyOrders_WhenNoOrders_ReturnsEmptyList()
        {
            _orders.Setup(x => x.GetOrdersByUser(2, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<Order>());

            Assert.Empty(await _service.GetMyOrders(2));
        }

        [Theory]
        [InlineData(24 * 60 + 1, true)]   // 24h01m before -> can cancel
        [InlineData(24 * 60, false)]      // exactly 24h   -> too late
        [InlineData(23 * 60, false)]
        [InlineData(-60, false)]          // already started
        public async Task GetMyOrders_CanCancel_FollowsTheTwentyFourHourRule(int minutesUntilEvent, bool expected)
        {
            _orders.Setup(x => x.GetOrdersByUser(2, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<Order> { MakeOrder(Now.AddMinutes(minutesUntilEvent)) });

            var dto = Assert.Single(await _service.GetMyOrders(2));

            Assert.Equal(expected, dto.CanCancel);
        }

        [Fact]
        public async Task GetMyOrders_WhenEventWasCancelled_CanCancelIsFalse()
        {
            _orders.Setup(x => x.GetOrdersByUser(2, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<Order> { MakeOrder(Now.AddDays(10), eventCancelled: true) });

            var dto = Assert.Single(await _service.GetMyOrders(2));

            Assert.True(dto.EventIsCancelled);
            Assert.False(dto.CanCancel);
        }

        // ---------- CancelOrder ----------

        [Fact]
        public async Task CancelOrder_MoreThan24HoursBefore_RemovesOrderAndFreesSeat()
        {
            var order = MakeOrder(Now.AddHours(24).AddMinutes(1));
            var eventSeat = SetupCancellable(order);

            var result = await _service.CancelOrder(order.Id, userId: 2);

            Assert.Equal(OrderResultStatus.Success, result.Status);
            Assert.True(eventSeat.IsAvailable);
            _orders.Verify(x => x.Remove(order), Times.Once);
            _orders.Verify(x => x.Save(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(24 * 60)]     // exactly 24h is NOT "more than 24h"
        [InlineData(24 * 60 - 1)]
        [InlineData(60)]
        [InlineData(-30)]         // event already started
        public async Task CancelOrder_24HoursOrLessBefore_ReturnsConflictAndChangesNothing(int minutesUntilEvent)
        {
            var order = MakeOrder(Now.AddMinutes(minutesUntilEvent));
            var eventSeat = SetupCancellable(order);

            var result = await _service.CancelOrder(order.Id, userId: 2);

            Assert.Equal(OrderResultStatus.Conflict, result.Status);
            Assert.False(eventSeat.IsAvailable);
            _orders.Verify(x => x.Remove(It.IsAny<Order>()), Times.Never);
            _orders.Verify(x => x.Save(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderBelongsToAnotherUser_ReturnsNotFound()
        {
            var order = MakeOrder(Now.AddDays(10), userId: 99);
            var eventSeat = SetupCancellable(order);

            var result = await _service.CancelOrder(order.Id, userId: 2);

            Assert.Equal(OrderResultStatus.NotFound, result.Status);
            Assert.False(eventSeat.IsAvailable);
            _orders.Verify(x => x.Remove(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderDoesNotExist_ReturnsNotFound()
        {
            _orders.Setup(x => x.GetByIdForCancellation(404, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Order?)null);

            var result = await _service.CancelOrder(404, userId: 2);

            Assert.Equal(OrderResultStatus.NotFound, result.Status);
            _orders.Verify(x => x.Save(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrder_WhenEventAlreadyCancelled_ReturnsConflict()
        {
            var order = MakeOrder(Now.AddDays(10), eventCancelled: true);
            SetupCancellable(order);

            var result = await _service.CancelOrder(order.Id, userId: 2);

            Assert.Equal(OrderResultStatus.Conflict, result.Status);
            _orders.Verify(x => x.Remove(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrder_WhenEventDateKindIsUnspecified_TreatsItAsUtc()
        {
            // EF returns DateTime with Kind=Unspecified; it must be interpreted as UTC.
            var order = MakeOrder(DateTime.SpecifyKind(Now.AddHours(25), DateTimeKind.Unspecified));
            SetupCancellable(order);

            var result = await _service.CancelOrder(order.Id, userId: 2);

            Assert.Equal(OrderResultStatus.Success, result.Status);
        }

        [Fact]
        public async Task CancelOrder_OnConcurrencyConflict_ReturnsConflict()
        {
            var order = MakeOrder(Now.AddDays(10));
            SetupCancellable(order);
            _orders.Setup(x => x.Save(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new ConcurrencyException("conflict", new Exception()));

            var result = await _service.CancelOrder(order.Id, userId: 2);

            Assert.Equal(OrderResultStatus.Conflict, result.Status);
        }
    }
}
