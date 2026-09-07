//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Moq;
//using SystemSalesTickets.Core.DTOs;
//using SystemSalesTickets.Core.Models;
//using SystemSalesTickets.Core.Repository;
//using SystemSalesTickets.Service.Service;

//namespace SystemSalesTickets.Tests
//{
//    public class OrderServiceTests
//    {
//        private readonly Mock<IOrderRepository> _orderRepositoryMock;
//        private readonly Mock<ISeatRepository> _seatRepositoryMock;
//        private readonly Mock<IMapper> _mapperMock;
//        private readonly Mock<ILogger<OrderService>> _loggerMock;

//        private readonly OrderService _service;

//        public OrderServiceTests()
//        {
//            _orderRepositoryMock = new Mock<IOrderRepository>();
//            _seatRepositoryMock = new Mock<ISeatRepository>();
//            _mapperMock = new Mock<IMapper>();
//            _loggerMock = new Mock<ILogger<OrderService>>();

//            _service = new OrderService(
//                _orderRepositoryMock.Object,
//                _mapperMock.Object,
//                _loggerMock.Object,
//                _seatRepositoryMock.Object
//            );
//        }

//        // ---------------------------------------------------------
//        // AddOrder - Seat not found
//        // ---------------------------------------------------------

//        [Fact]
//        public async Task AddOrder_WhenSeatDoesNotExist_ReturnsSeatNotFound()
//        {
//            // Arrange
//            var orderDto = new OrderDTO
//            {
//                SeatDTO = new Seat
//                {
//                    SeatId = 1
//                }
//            };

//            var order = new Order
//            {
//                Seat = new Seat
//                {
//                    SeatId = 1
//                }
//            };

//            _mapperMock
//                .Setup(x => x.Map<Order>(orderDto))
//                .Returns(order);

//            _seatRepositoryMock
//                .Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync((Seat)null);

//            // Act
//            var result = await _service.AddOrder(orderDto);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal("Seat not found", result.Message);

//            _seatRepositoryMock.Verify(
//                x => x.GetById(1, It.IsAny<CancellationToken>()),
//                Times.Once);

//            _seatRepositoryMock.Verify(
//                x => x.Update(It.IsAny<Seat>(), It.IsAny<CancellationToken>()),
//                Times.Never);

//            _orderRepositoryMock.Verify(
//                x => x.Add(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }


//        // ---------------------------------------------------------
//        // AddOrder - Seat already occupied
//        // ---------------------------------------------------------

//        [Fact]
//        public async Task AddOrder_WhenSeatIsOccupied_ReturnsSeatAlreadyOccupied()
//        {
//            // Arrange
//            var orderDto = new OrderDTO
//            {
//                SeatDTO = new Seat
//                {
//                    SeatId = 1
//                }
//            };

//            var order = new Order
//            {
//                Seat = new Seat
//                {
//                    SeatId = 1
//                }
//            };

//            var seat = new Seat
//            {
//                SeatId = 1,
//                IsAvailable = false
//            };

//            _mapperMock
//                .Setup(x => x.Map<Order>(orderDto))
//                .Returns(order);

//            _seatRepositoryMock
//                .Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(seat);

//            // Act
//            var result = await _service.AddOrder(orderDto);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal("Seat is already occupied", result.Message);

//            _seatRepositoryMock.Verify(
//                x => x.Update(It.IsAny<Seat>(), It.IsAny<CancellationToken>()),
//                Times.Never);

//            _orderRepositoryMock.Verify(
//                x => x.Add(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }


//        // ---------------------------------------------------------
//        // AddOrder - Success
//        // ---------------------------------------------------------

//        [Fact]
//        public async Task AddOrder_WhenSeatIsAvailable_AddsOrderAndReturnsResult()
//        {
//            // Arrange
//            var orderDto = new OrderDTO
//            {
//                SeatDTO = new Seat
//                {
//                    SeatId = 1
//                }
//            };
//            var order = new Order
//            {
//                Seat = new Seat
//                {
//                    SeatId = 1
//                }
//            };

//            var seat = new Seat
//            {
//                SeatId = 1,
//                IsAvailable = true
//            };

//            var expectedResult = new OrderLogDTO
//            {
//                Message = "Success"
//            };

//            _mapperMock
//                .Setup(x => x.Map<Order>(orderDto))
//                .Returns(order);

//            _seatRepositoryMock
//                .Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(seat);

//            _seatRepositoryMock
//                .Setup(x => x.Update(seat, It.IsAny<CancellationToken>()))
//                .Returns(Task.CompletedTask);

//            _orderRepositoryMock
//                .Setup(x => x.Add(order, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(order);

//            _mapperMock
//                .Setup(x => x.Map<OrderLogDTO>(order))
//                .Returns(expectedResult);

//            // Act
//            var result = await _service.AddOrder(orderDto);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(expectedResult, result);

//            // הכיסא צריך להפוך לתפוס
//            Assert.False(seat.IsAvailable);

//            // Update של הכיסא בוצע
//            _seatRepositoryMock.Verify(
//                x => x.Update(seat, It.IsAny<CancellationToken>()),
//                Times.Once);

//            // הוספת ההזמנה בוצעה
//            _orderRepositoryMock.Verify(
//                x => x.Add(order, It.IsAny<CancellationToken>()),
//                Times.Once);
//        }


//        // ---------------------------------------------------------
//        // AddOrder - Exception / Concurrent booking
//        // ---------------------------------------------------------

//        [Fact]
//        public async Task AddOrder_WhenRepositoryThrows_ReturnsConcurrentBookingMessage()
//        {
//            // Arrange
//            var orderDto = new OrderDTO
//            {
//                SeatDTO = new Seat
//                {
//                    SeatId = 1
//                }
//            };

//            var order = new Order
//            {
//                Seat = new Seat
//                {
//                    SeatId = 1
//                }
//            };

//            var seat = new Seat
//            {
//                SeatId = 1,
//                IsAvailable = true
//            };

//            _mapperMock
//                .Setup(x => x.Map<Order>(orderDto))
//                .Returns(order);

//            _seatRepositoryMock
//                .Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(seat);

//            _seatRepositoryMock
//                .Setup(x => x.Update(seat, It.IsAny<CancellationToken>()))
//                .ThrowsAsync(new Exception());

//            // Act
//            var result = await _service.AddOrder(orderDto);

//            // Assert
//            Assert.NotNull(result);

//            Assert.Equal(
//                "Seat was just booked by someone else, please try again",
//                result.Message);

//            _seatRepositoryMock.Verify(
//                x => x.Update(seat, It.IsAny<CancellationToken>()),
//                Times.Once);

//            // בגלל שה-Update זרק Exception,
//            // Add לא אמור להתבצע
//            _orderRepositoryMock.Verify(
//                x => x.Add(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
//                Times.Never);
//        }


//        // ---------------------------------------------------------
//        // GetAllOrders
//        // ---------------------------------------------------------

//        [Fact]
//        public async Task GetAllOrders_ReturnsMappedOrders()
//        {
//            // Arrange
//            var orders = new List<Order>
//            {
//                new Order(),
//                new Order()
//            };

//            var expectedResult = new List<OrderDTO>
//            {
//                new OrderDTO(),
//                new OrderDTO()
//            };

//            _orderRepositoryMock
//                .Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(orders);

//            _mapperMock
//                .Setup(x => x.Map<IEnumerable<OrderDTO>>(orders))
//                .Returns(expectedResult);

//            // Act
//            var result = await _service.GetAllOrders();

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(expectedResult, result);

//            _orderRepositoryMock.Verify(
//                x => x.GetAll(It.IsAny<CancellationToken>()),
//                Times.Once);
//        }


//        // ---------------------------------------------------------
//        // GetOrderById
//        // ---------------------------------------------------------

//        [Fact]
//        public async Task GetOrderById_WhenOrderExists_ReturnsMappedOrder()
//        {
//            // Arrange
//            int id = 1;

//            var order = new Order
//            {
//                OrderId = id
//            };

//            var expectedResult = new OrderLogDTO
//            {
//                Message = "Order found"
//            };

//            _orderRepositoryMock
//                .Setup(x => x.GetById(id, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(order);

//            _mapperMock
//                .Setup(x => x.Map<OrderLogDTO>(order))
//                .Returns(expectedResult);

//            // Act
//            var result = await _service.GetOrderById(id);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(expectedResult, result);

//            _orderRepositoryMock.Verify(
//                x => x.GetById(id, It.IsAny<CancellationToken>()),
//                Times.Once);
//        }

//        [Fact]
//        public async Task AddOrder_ConcurrentRequestsForSameSeat_OneSucceedsAndOneReturnsConcurrencyMessage()
//        {
//            // 1. Arrange - הגדרת מסד נתונים זמני התומך ב-Concurrency
//            var options = new DbContextOptionsBuilder<DataContext>()
//                .UseSqlite("DataSource=:memory:")
//                .Options;

//            using (var context = new DataContext(options))
//            {
//                await context.Database.OpenConnectionAsync();
//                await context.Database.EnsureCreatedAsync();

//                // הכנסת כיסא זמין התחלתי
//                context.Seats.Add(new Seat { Id = 1, IsAvailable = true, Row = 1, Line = 1 });
//                await context.SaveChangesAsync();
//            }

//            var mapperMock = new Mock<IMapper>();
//            var loggerMock = new Mock<ILogger<OrderService>>();

//            // הגדרת מיפוי בסיסי עבור הטסט
//            mapperMock.Setup(m => m.Map<Order>(It.IsAny<OrderDTO>()))
//                .Returns((OrderDTO dto) => new Order { SeatId = dto.SeatId });

//            var orderDto1 = new OrderDTO { SeatId = 1 };
//            var orderDto2 = new OrderDTO { SeatId = 1 };

//            // 2. Act - הרצת שתי בקשות במקביל
//            Task<OrderLogDTO> task1;
//            Task<OrderLogDTO> task2;

//            using (var context1 = new DataContext(options))
//            using (var context2 = new DataContext(options))
//            {
//                var repo1 = new SeatRepository(context1);
//                var orderRepo1 = new OrderRepository(context1);
//                var service1 = new OrderService(repo1, orderRepo1, mapperMock.Object, loggerMock.Object, context1);

//                var repo2 = new SeatRepository(context2);
//                var orderRepo2 = new OrderRepository(context2);
//                var service2 = new OrderService(repo2, orderRepo2, mapperMock.Object, loggerMock.Object, context2);

//                // הפעלת שתי הקריאות במקביל
//                task1 = service1.AddOrder(orderDto1, CancellationToken.None);
//                task2 = service2.AddOrder(orderDto2, CancellationToken.None);

//                await Task.WhenAll(task1, task2);
//            }

//            var result1 = await task1;
//            var result2 = await task2;

//            // 3. Assert - בדיקה שאחת מהבקשות נתקלה בהודעת ה-Concurrency או שהכיסא נתפס
//            bool hasConcurrencyWarning =
//                result1.Message == "Seat was just booked by someone else, please try again" ||
//                result2.Message == "Seat was just booked by someone else, please try again" ||
//                result1.Message == "Seat is already occupied" ||
//                result2.Message == "Seat is already occupied";

//            Assert.True(hasConcurrencyWarning);
//        }
//    }
//}
