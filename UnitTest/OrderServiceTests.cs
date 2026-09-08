using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;
using MyApp.Application.Common.Models;

namespace SystemSalesTickets.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IEventSeatRepository> _eventSeatRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;

        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _eventSeatRepositoryMock = new Mock<IEventSeatRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<OrderService>>();

            _service = new OrderService(
                _orderRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _eventSeatRepositoryMock.Object);
        }

        // =====================================================
        // AddOrder
        // =====================================================

        [Fact]
        public async Task AddOrder_WhenEventSeatDoesNotExist_ReturnsSeatNotFound()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                EventId = 1,
                SeatId = 999,
                UserId = 2
            };

            _eventSeatRepositoryMock
                .Setup(x => x.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventSeat?)null);

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Seat not found", result.Message);

            _eventSeatRepositoryMock.Verify(
                x => x.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.Add(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Save(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _mapperMock.Verify(
                x => x.Map<Order>(
                    It.IsAny<OrderDTO>()),
                Times.Never);
        }

        [Fact]
        public async Task AddOrder_WhenEventSeatIsAlreadyOccupied_ReturnsConflictMessage()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                EventId = 1,
                SeatId = 1,
                UserId = 2
            };

            var eventSeat = new EventSeat
            {
                EventId = 1,
                SeatId = 1,
                IsAvailable = false,
                Version = Guid.NewGuid()
            };

            _eventSeatRepositoryMock
                .Setup(x => x.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventSeat);

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(
                "Seat is already occupied",
                result.Message);

            _eventSeatRepositoryMock.Verify(
                x => x.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.Add(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Save(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddOrder_WhenEventSeatIsAvailable_AddsOrderAndSaves()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                EventId = 1,
                SeatId = 2,
                UserId = 2
            };

            var eventSeat = new EventSeat
            {
                EventId = 1,
                SeatId = 2,
                IsAvailable = true,
                Version = Guid.NewGuid()
            };

            var newOrder = new Order
            {
                OrderId = 10,
                EventId = 1,
                SeatId = 2,
                UserId = 2,
                EventName = "Concert A",
                OrderDate = DateTime.UtcNow
            };

            var expected = new OrderLogDTO
            {
                Id = 10,
                EventName = "Concert A"
            };

            _eventSeatRepositoryMock
                .Setup(x => x.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventSeat);

            _mapperMock
                .Setup(x => x.Map<Order>(orderDto))
                .Returns(newOrder);

            _orderRepositoryMock
                .Setup(x => x.Add(
                    newOrder,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newOrder);

            _orderRepositoryMock
                .Setup(x => x.Save(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(x => x.Map<OrderLogDTO>(newOrder))
                .Returns(expected);

            // Act
            var result = await _service.AddOrder(
                orderDto,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            // ה־EventSeat חייב להפוך ללא זמין
            Assert.False(eventSeat.IsAvailable);

            _eventSeatRepositoryMock.Verify(
                x => x.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<Order>(orderDto),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.Add(
                    newOrder,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.Save(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<OrderLogDTO>(newOrder),
                Times.Once);
        }

        [Fact]
        public async Task AddOrder_WhenConcurrencyExceptionOccurs_ReturnsConflictMessage()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                EventId = 1,
                SeatId = 2,
                UserId = 2
            };

            var eventSeat = new EventSeat
            {
                EventId = 1,
                SeatId = 2,
                IsAvailable = true,
                Version = Guid.NewGuid()
            };

            var newOrder = new Order
            {
                OrderId = 10,
                EventId = 1,
                SeatId = 2,
                UserId = 2,
                EventName = "Concert A",
                OrderDate = DateTime.UtcNow
            };

            _eventSeatRepositoryMock
                .Setup(x => x.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventSeat);

            _mapperMock
                .Setup(x => x.Map<Order>(orderDto))
                .Returns(newOrder);

            _orderRepositoryMock
                .Setup(x => x.Add(
                    newOrder,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newOrder);

            _orderRepositoryMock
                .Setup(x => x.Save(
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new DbUpdateConcurrencyException());

            // Act
            var result = await _service.AddOrder(
                orderDto,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                "Seat was just booked by someone else, please try again",
                result.Message);

            _eventSeatRepositoryMock.Verify(
                x => x.GetByEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.Add(
                    newOrder,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.Save(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // בגלל ה־Concurrency לא אמורה להיות המרה ל־DTO הצלחה
            _mapperMock.Verify(
                x => x.Map<OrderLogDTO>(
                    It.IsAny<Order>()),
                Times.Never);
        }

        // =====================================================
        // GetAllOrders
        // =====================================================

        [Fact]
        public async Task GetAllOrders_ReturnsOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = 1,
                    EventId = 1,
                    SeatId = 1,
                    EventName = "Concert A",
                    OrderDate = DateTime.UtcNow
                },
                new Order
                {
                    OrderId = 2,
                    EventId = 1,
                    SeatId = 2,
                    EventName = "Concert A",
                    OrderDate = DateTime.UtcNow
                }
            };

            var orderDtos = new List<OrderDTO>
            {
                new OrderDTO
                {
                    EventId = 1,
                    SeatId = 1,
                    UserId = 2
                },
                new OrderDTO
                {
                    EventId = 1,
                    SeatId = 2,
                    UserId = 2
                }
            };

            _orderRepositoryMock
                .Setup(x => x.GetAllAsync(
                    1,
                    20,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PagedResponse<Order>(
                        orders,
                        1,
                        20,
                        orders.Count));

            _mapperMock
                .Setup(x => x.Map<IEnumerable<OrderDTO>>(orders))
                .Returns(orderDtos);

            // Act
            var result = await _service.GetAllOrders(
                1,
                20,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());

            _orderRepositoryMock.Verify(
                x => x.GetAllAsync(
                    1,
                    20,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<IEnumerable<OrderDTO>>(orders),
                Times.Once);
        }

        [Fact]
        public async Task GetAllOrders_UsesRequestedPagination()
        {
            // Arrange
            var orders = new List<Order>();

            _orderRepositoryMock
                .Setup(x => x.GetAllAsync(
                    2,
                    10,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PagedResponse<Order>(
                        orders,
                        2,
                        10,
                        15));

            _mapperMock
                .Setup(x => x.Map<IEnumerable<OrderDTO>>(orders))
                .Returns(new List<OrderDTO>());

            // Act
            var result = await _service.GetAllOrders(
                2,
                10,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(2, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(15, result.TotalRecords);

            _orderRepositoryMock.Verify(
                x => x.GetAllAsync(
                    2,
                    10,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // =====================================================
        // GetOrderById
        // =====================================================

        [Fact]
        public async Task GetOrderById_WhenOrderExists_ReturnsOrderLogDTO()
        {
            // Arrange
            const int id = 1;

            var order = new Order
            {
                OrderId = id,
                EventId = 1,
                SeatId = 1,
                UserId = 2,
                EventName = "Concert A",
                OrderDate = DateTime.UtcNow
            };

            var expected = new OrderLogDTO
            {
                Id = id,
                EventName = "Concert A"
            };

            _orderRepositoryMock
                .Setup(x => x.GetById(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mapperMock
                .Setup(x => x.Map<OrderLogDTO>(order))
                .Returns(expected);

            // Act
            var result = await _service.GetOrderById(
                id,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _orderRepositoryMock.Verify(
                x => x.GetById(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<OrderLogDTO>(order),
                Times.Once);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_ReturnsNull()
        {
            // Arrange
            const int id = 999;

            _orderRepositoryMock
                .Setup(x => x.GetById(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            _mapperMock
                .Setup(x => x.Map<OrderLogDTO>(null))
                .Returns((OrderLogDTO?)null);

            // Act
            var result = await _service.GetOrderById(
                id,
                CancellationToken.None);

            // Assert
            Assert.Null(result);

            _orderRepositoryMock.Verify(
                x => x.GetById(
                    id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<OrderLogDTO>(null),
                Times.Once);
        }
    }
}
