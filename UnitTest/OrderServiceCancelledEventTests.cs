using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;

namespace UnitTest
{
    public class OrderServiceCancelledEventTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<IEventSeatRepository> _eventSeatRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IOrderConfirmationEmailService> _emailMock = new();
        private readonly OrderService _service;

        public OrderServiceCancelledEventTests()
        {
            _service = new OrderService(
                _orderRepositoryMock.Object,
                new Mock<IMapper>().Object,
                new Mock<ILogger<OrderService>>().Object,
                _eventSeatRepositoryMock.Object,
                _userRepositoryMock.Object,
                _emailMock.Object);
        }

        [Fact]
        public async Task AddOrder_WhenEventIsCancelled_ReturnsConflictAndDoesNotBook()
        {
            var orderDto = new OrderDTO { EventId = 1, SeatId = 2, UserId = 2 };
            var eventSeat = new EventSeat
            {
                EventId = 1,
                SeatId = 2,
                IsAvailable = true,
                Event = new Event { Id = 1, Name = "Concert A", IsCancelled = true }
            };

            _eventSeatRepositoryMock
                .Setup(x => x.GetByEventAndSeat(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventSeat);

            var result = await _service.AddOrder(orderDto);

            Assert.Equal(OrderResultStatus.Conflict, result.Status);
            Assert.Equal("This event was cancelled", result.Message);
            Assert.True(eventSeat.IsAvailable);

            _orderRepositoryMock.Verify(
                x => x.Add(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
            _orderRepositoryMock.Verify(
                x => x.Save(It.IsAny<CancellationToken>()), Times.Never);
            _emailMock.Verify(
                x => x.SendAsync(It.IsAny<Order>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
