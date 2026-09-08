using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;

namespace SystemSalesTickets.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<ISeatRepository> _seatRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;

    private readonly OrderService _service;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _seatRepositoryMock = new Mock<ISeatRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<OrderService>>();

            //_service = new OrderService(
            //    _orderRepositoryMock.Object,
            //    _mapperMock.Object,
            //    _loggerMock.Object
            //    );
        }


        // =====================================================
        // AddOrder
        // =====================================================

        [Fact]
        public async Task AddOrder_WhenSeatDoesNotExist_ReturnsSeatNotFound()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                SeatId = 999
            };

            _seatRepositoryMock
                .Setup(x => x.GetById(
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Seat?)null);

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Seat not found", result.Message);

            _seatRepositoryMock.Verify(
                x => x.GetById(
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _seatRepositoryMock.Verify(
                x => x.Update(
                    It.IsAny<Seat>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Add(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task AddOrder_WhenSeatIsAlreadyOccupied_ReturnsConflictMessage()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                SeatId = 1
            };

            var seat = new Seat
            {
                SeatId = 1,
                Row = 1,
                Line = 1,
            };

            _seatRepositoryMock
                .Setup(x => x.GetById(
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(seat);

            _orderRepositoryMock
                .Setup(x => x.ExistsForEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(
                "Seat is already occupied",
                result.Message);

            _seatRepositoryMock.Verify(
                x => x.GetById(
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.ExistsForEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _seatRepositoryMock.Verify(
                x => x.Update(
                    It.IsAny<Seat>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Add(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddOrder_WhenSeatIsUnavailableForAnotherEvent_AddsOrder()
        {
            var orderDto = new OrderDTO
            {
                EventId = 2,
                SeatId = 1
            };
            var seat = new Seat
            {
                SeatId = 1,
            };
            var newOrder = new Order
            {
                EventId = orderDto.EventId,
                SeatId = orderDto.SeatId
            };

            _seatRepositoryMock
                .Setup(x => x.GetById(orderDto.SeatId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seat);
            _orderRepositoryMock
                .Setup(x => x.ExistsForEventAndSeat(
                    orderDto.EventId,
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mapperMock.Setup(x => x.Map<Order>(orderDto)).Returns(newOrder);
            _orderRepositoryMock
                .Setup(x => x.Add(newOrder, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newOrder);
            _mapperMock
                .Setup(x => x.Map<OrderLogDTO>(newOrder))
                .Returns(new OrderLogDTO());

            var result = await _service.AddOrder(orderDto);

            Assert.NotNull(result);
            _orderRepositoryMock.Verify(
                x => x.Add(newOrder, It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task AddOrder_WhenSeatIsAvailable_AddsOrderAndSaves()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                SeatId = 1
            };

            var seat = new Seat
            {
                SeatId = 1,
                Row = 1,
                Line = 1,
            };

            var newOrder = new Order
            {
                OrderId = 10,
                SeatId = 1,
                EventId = 1,
                EventName = "Concert A",
                OrderDate = DateTime.UtcNow
            };

            var expected = new OrderLogDTO
            {
                Id = 10,
                EventName = "Concert A"
            };

            _seatRepositoryMock
                .Setup(x => x.GetById(
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(seat);

            _mapperMock
                .Setup(x => x.Map<Order>(orderDto))
                .Returns(newOrder);

            _orderRepositoryMock
                .Setup(x => x.Add(
                    newOrder,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newOrder);

            _mapperMock
                .Setup(x => x.Map<OrderLogDTO>(newOrder))
                .Returns(expected);

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            _seatRepositoryMock.Verify(
                x => x.GetById(
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _seatRepositoryMock.Verify(
                x => x.Update(
                    It.IsAny<Seat>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Add(
                    newOrder,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // חשוב: Save אחד בלבד
            _orderRepositoryMock.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<Order>(orderDto),
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
                SeatId = 1
            };

            var seat = new Seat
            {
                SeatId = 1,
                Row = 1,
                Line = 1,
            };

            var newOrder = new Order
            {
                OrderId = 10,
                SeatId = 1,
                EventId = 1,
                EventName = "Concert A",
                OrderDate = DateTime.UtcNow
            };

            _seatRepositoryMock
                .Setup(x => x.GetById(
                    orderDto.SeatId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(seat);

            _mapperMock
                .Setup(x => x.Map<Order>(orderDto))
                .Returns(newOrder);

            _orderRepositoryMock
                .Setup(x => x.Add(
                    newOrder,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newOrder);

            _orderRepositoryMock
                .Setup(x => x.Save(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                "Seat was just booked by someone else, please try again",
                result.Message);

            _orderRepositoryMock.Verify(
                x => x.Add(
                    newOrder,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Once);

            _mapperMock.Verify(
                x => x.Map<OrderLogDTO>(It.IsAny<Order>()),
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
                SeatId = 1
            },
            new OrderDTO
            {
                SeatId = 2
            }
        };

            _orderRepositoryMock
                .Setup(x => x.GetAllAsync(
                    1,
                    20,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new MyApp.Application.Common.Models.PagedResponse<Order>(
                        orders,
                        1,
                        20,
                        orders.Count));

            _mapperMock
                .Setup(x => x.Map<IEnumerable<OrderDTO>>(orders))
                .Returns(orderDtos);

            // Act
            var result = await _service.GetAllOrders();

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
                    new MyApp.Application.Common.Models.PagedResponse<Order>(
                        orders,
                        2,
                        10,
                        15));

            _mapperMock
                .Setup(x => x.Map<IEnumerable<OrderDTO>>(orders))
                .Returns(new List<OrderDTO>());

            // Act
            var result = await _service.GetAllOrders(2, 10);

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
            var result = await _service.GetOrderById(id);

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
            var result = await _service.GetOrderById(id);

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
