using AutoMapper;
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

            _service = new OrderService(
                _orderRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _seatRepositoryMock.Object
            );
        }

        // ---------------------------------------------------------
        // AddOrder - Seat not found
        // ---------------------------------------------------------

        [Fact]
        public async Task AddOrder_WhenSeatDoesNotExist_ReturnsSeatNotFound()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                SeatDTO = new Seat
                {
                    SeatId = 1
                }
            };

            var order = new Order
            {
                Seat = new Seat
                {
                    SeatId = 1
                }
            };

            _mapperMock
                .Setup(x => x.Map<Order>(orderDto))
                .Returns(order);

            _seatRepositoryMock
                .Setup(x => x.GetById(1))
                .ReturnsAsync((Seat)null);

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Seat not found", result.Message);

            _seatRepositoryMock.Verify(
                x => x.GetById(1),
                Times.Once);

            _seatRepositoryMock.Verify(
                x => x.Update(It.IsAny<Seat>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Add(It.IsAny<Order>()),
                Times.Never);
        }


        // ---------------------------------------------------------
        // AddOrder - Seat already occupied
        // ---------------------------------------------------------

        [Fact]
        public async Task AddOrder_WhenSeatIsOccupied_ReturnsSeatAlreadyOccupied()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                SeatDTO = new Seat
                {
                    SeatId = 1
                }
            };

            var order = new Order
            {
                Seat = new Seat
                {
                    SeatId = 1
                }
            };

            var seat = new Seat
            {
                SeatId = 1,
                IsAvailable = false
            };

            _mapperMock
                .Setup(x => x.Map<Order>(orderDto))
                .Returns(order);

            _seatRepositoryMock
                .Setup(x => x.GetById(1))
                .ReturnsAsync(seat);

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Seat is already occupied", result.Message);

            _seatRepositoryMock.Verify(
                x => x.Update(It.IsAny<Seat>()),
                Times.Never);

            _orderRepositoryMock.Verify(
                x => x.Add(It.IsAny<Order>()),
                Times.Never);
        }


        // ---------------------------------------------------------
        // AddOrder - Success
        // ---------------------------------------------------------

        [Fact]
        public async Task AddOrder_WhenSeatIsAvailable_AddsOrderAndReturnsResult()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                SeatDTO = new Seat
                {
                    SeatId = 1
                }
            };
            var order = new Order
            {
                Seat = new Seat
                {
                    SeatId = 1
                }
            };

            var seat = new Seat
            {
                SeatId = 1,
                IsAvailable = true
            };

            var expectedResult = new OrderLogDTO
            {
                Message = "Success"
            };

            _mapperMock
                .Setup(x => x.Map<Order>(orderDto))
                .Returns(order);

            _seatRepositoryMock
                .Setup(x => x.GetById(1))
                .ReturnsAsync(seat);

            _seatRepositoryMock
                .Setup(x => x.Update(seat))
                .Returns(Task.CompletedTask);

            _orderRepositoryMock
                .Setup(x => x.Add(order))
                .ReturnsAsync(order);

            _mapperMock
                .Setup(x => x.Map<OrderLogDTO>(order))
                .Returns(expectedResult);

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);

            // הכיסא צריך להפוך לתפוס
            Assert.False(seat.IsAvailable);

            // Update של הכיסא בוצע
            _seatRepositoryMock.Verify(
                x => x.Update(seat),
                Times.Once);

            // הוספת ההזמנה בוצעה
            _orderRepositoryMock.Verify(
                x => x.Add(order),
                Times.Once);
        }


        // ---------------------------------------------------------
        // AddOrder - Exception / Concurrent booking
        // ---------------------------------------------------------

        [Fact]
        public async Task AddOrder_WhenRepositoryThrows_ReturnsConcurrentBookingMessage()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                SeatDTO = new Seat
                {
                    SeatId = 1
                }
            };

            var order = new Order
            {
                Seat = new Seat
                {
                    SeatId = 1
                }
            };

            var seat = new Seat
            {
                SeatId = 1,
                IsAvailable = true
            };

            _mapperMock
                .Setup(x => x.Map<Order>(orderDto))
                .Returns(order);

            _seatRepositoryMock
                .Setup(x => x.GetById(1))
                .ReturnsAsync(seat);

            _seatRepositoryMock
                .Setup(x => x.Update(seat))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _service.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                "Seat was just booked by someone else, please try again",
                result.Message);

            _seatRepositoryMock.Verify(
                x => x.Update(seat),
                Times.Once);

            // בגלל שה-Update זרק Exception,
            // Add לא אמור להתבצע
            _orderRepositoryMock.Verify(
                x => x.Add(It.IsAny<Order>()),
                Times.Never);
        }


        // ---------------------------------------------------------
        // GetAllOrders
        // ---------------------------------------------------------

        [Fact]
        public async Task GetAllOrders_ReturnsMappedOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order(),
                new Order()
            };

            var expectedResult = new List<OrderDTO>
            {
                new OrderDTO(),
                new OrderDTO()
            };

            _orderRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(orders);

            _mapperMock
                .Setup(x => x.Map<IEnumerable<OrderDTO>>(orders))
                .Returns(expectedResult);

            // Act
            var result = await _service.GetAllOrders();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);

            _orderRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Once);
        }


        // ---------------------------------------------------------
        // GetOrderById
        // ---------------------------------------------------------

        [Fact]
        public async Task GetOrderById_WhenOrderExists_ReturnsMappedOrder()
        {
            // Arrange
            int id = 1;

            var order = new Order
            {
                OrderId = id
            };

            var expectedResult = new OrderLogDTO
            {
                Message = "Order found"
            };

            _orderRepositoryMock
                .Setup(x => x.GetById(id))
                .ReturnsAsync(order);

            _mapperMock
                .Setup(x => x.Map<OrderLogDTO>(order))
                .Returns(expectedResult);

            // Act
            var result = await _service.GetOrderById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);

            _orderRepositoryMock.Verify(
                x => x.GetById(id),
                Times.Once);
        }
    }
}
