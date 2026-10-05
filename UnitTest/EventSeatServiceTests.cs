using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;

namespace UnitTest
{
    public class EventSeatServiceTests
    {
        private readonly Mock<IEventSeatRepository> _eventSeatRepository;
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<ISeatRepository> _seatRepository;
        private readonly Mock<IOrderRepository> _orderRepository;
        private readonly Mock<ILogger<EventSeatService>> _logger;
        private readonly EventSeatService _service;

        public EventSeatServiceTests()
        {
            _eventSeatRepository = new Mock<IEventSeatRepository>();
            _eventRepository = new Mock<IEventRepository>();
            _seatRepository = new Mock<ISeatRepository>();
            _orderRepository = new Mock<IOrderRepository>();
            _logger = new Mock<ILogger<EventSeatService>>();

            _service = new EventSeatService(
                _eventSeatRepository.Object,
                _eventRepository.Object,
                _seatRepository.Object,
                _orderRepository.Object,
                _logger.Object);
        }

        [Fact]
        public async Task GetSeatsForEvent_ReturnsMappedSeats()
        {
            var eventSeat = new EventSeat
            {
                EventId = 1,
                SeatId = 2,
                IsAvailable = true,
                Version = Guid.NewGuid(),
                Seat = new Seat { Id = 2, Row = 3, Line = 4 }
            };

            _eventSeatRepository
                .Setup(x => x.GetAllByEvent(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventSeat> { eventSeat });

            var result = await _service.GetSeatsForEvent(1, CancellationToken.None);

            var item = Assert.Single(result);
            Assert.Equal(1, item.EventId);
            Assert.Equal(2, item.SeatId);
            Assert.Equal(3, item.Row);
            Assert.Equal(4, item.Line);
            Assert.True(item.IsAvailable);
            Assert.Equal(eventSeat.Version, item.Version);
        }

        [Fact]
        public async Task AddEventSeat_WhenEventDoesNotExist_ReturnsNotFound()
        {
            _eventRepository
                .Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event?)null);

            var result = await _service.AddEventSeat(
                new AddEventSeatDTO { EventId = 1, SeatId = 2 },
                CancellationToken.None);

            Assert.Equal(OrderResultStatus.NotFound, result.Status);
            Assert.Equal("Event not found", result.Message);
            _seatRepository.Verify(
                x => x.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddEventSeat_WhenSeatDoesNotExist_ReturnsNotFound()
        {
            _eventRepository
                .Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event { Id = 1 });

            _seatRepository
                .Setup(x => x.GetById(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Seat?)null);

            var result = await _service.AddEventSeat(
                new AddEventSeatDTO { EventId = 1, SeatId = 2 },
                CancellationToken.None);

            Assert.Equal(OrderResultStatus.NotFound, result.Status);
            Assert.Equal("Seat not found", result.Message);
            _eventSeatRepository.Verify(
                x => x.GetByEventAndSeat(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddEventSeat_WhenLinkAlreadyExists_ReturnsConflict()
        {
            _eventRepository.Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event { Id = 1 });
            _seatRepository.Setup(x => x.GetById(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Seat { Id = 2, Row = 1, Line = 1 });
            _eventSeatRepository.Setup(x => x.GetByEventAndSeat(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventSeat());

            var result = await _service.AddEventSeat(
                new AddEventSeatDTO { EventId = 1, SeatId = 2 },
                CancellationToken.None);

            Assert.Equal(OrderResultStatus.Conflict, result.Status);
            _eventSeatRepository.Verify(
                x => x.Add(It.IsAny<EventSeat>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddEventSeat_WhenValid_AddsAndSaves()
        {
            var seat = new Seat { Id = 2, Row = 5, Line = 6 };
            _eventRepository.Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event { Id = 1 });
            _seatRepository.Setup(x => x.GetById(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seat);
            _eventSeatRepository.Setup(x => x.GetByEventAndSeat(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventSeat?)null);
            _eventSeatRepository.Setup(x => x.Add(It.IsAny<EventSeat>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventSeat es, CancellationToken _) => es);
            _eventSeatRepository.Setup(x => x.Save(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.AddEventSeat(
                new AddEventSeatDTO { EventId = 1, SeatId = 2 },
                CancellationToken.None);

            Assert.Equal(OrderResultStatus.Success, result.Status);
            Assert.NotNull(result.EventSeat);
            Assert.True(result.EventSeat.IsAvailable);
            Assert.Equal(5, result.EventSeat.Row);
            Assert.Equal(6, result.EventSeat.Line);
            _eventSeatRepository.Verify(
                x => x.Add(It.IsAny<EventSeat>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _eventSeatRepository.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoveEventSeat_WhenLinkDoesNotExist_ReturnsNotFound()
        {
            _eventSeatRepository.Setup(x => x.GetByEventAndSeat(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventSeat?)null);

            var result = await _service.RemoveEventSeat(1, 2, CancellationToken.None);

            Assert.Equal(OrderResultStatus.NotFound, result.Status);
            _orderRepository.Verify(
                x => x.ExistsForEventAndSeat(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RemoveEventSeat_WhenOrdered_ReturnsConflict()
        {
            _eventSeatRepository.Setup(x => x.GetByEventAndSeat(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventSeat());
            _orderRepository.Setup(x => x.ExistsForEventAndSeat(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.RemoveEventSeat(1, 2, CancellationToken.None);

            Assert.Equal(OrderResultStatus.Conflict, result.Status);
            _eventSeatRepository.Verify(x => x.Remove(It.IsAny<EventSeat>()), Times.Never);
        }

        [Fact]
        public async Task RemoveEventSeat_WhenValid_RemovesAndSaves()
        {
            var eventSeat = new EventSeat { EventId = 1, SeatId = 2 };
            _eventSeatRepository.Setup(x => x.GetByEventAndSeat(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventSeat);
            _orderRepository.Setup(x => x.ExistsForEventAndSeat(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _eventSeatRepository.Setup(x => x.Save(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.RemoveEventSeat(1, 2, CancellationToken.None);

            Assert.Equal(OrderResultStatus.Success, result.Status);
            _eventSeatRepository.Verify(x => x.Remove(eventSeat), Times.Once);
            _eventSeatRepository.Verify(x => x.Save(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LinkAllSeatsToEvent_WhenEventDoesNotExist_ReturnsNotFound()
        {
            _eventRepository.Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event?)null);

            var result = await _service.LinkAllSeatsToEvent(1, CancellationToken.None);

            Assert.Equal(OrderResultStatus.NotFound, result.Status);
            _seatRepository.Verify(
                x => x.GetAllSeats(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task LinkAllSeatsToEvent_LinksOnlyMissingSeats()
        {
            _eventRepository.Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event { Id = 1 });

            _seatRepository.Setup(x => x.GetAllSeats(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Seat>
                {
                    new Seat { Id = 1 },
                    new Seat { Id = 2 },
                    new Seat { Id = 3 }
                });

            _eventSeatRepository.Setup(x => x.GetAllByEvent(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventSeat>
                {
                    new EventSeat { EventId = 1, SeatId = 2 }
                });

            _eventSeatRepository.Setup(x => x.Add(It.IsAny<EventSeat>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventSeat es, CancellationToken _) => es);
            _eventSeatRepository.Setup(x => x.Save(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.LinkAllSeatsToEvent(1, CancellationToken.None);

            Assert.Equal(OrderResultStatus.Success, result.Status);
            Assert.Equal(2, result.LinkedCount);
            _eventSeatRepository.Verify(
                x => x.Add(It.Is<EventSeat>(es => es.SeatId == 1 || es.SeatId == 3), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
            _eventSeatRepository.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task LinkAllSeatsToEvent_WhenAllSeatsAlreadyLinked_DoesNotSave()
        {
            _eventRepository.Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event { Id = 1 });
            _seatRepository.Setup(x => x.GetAllSeats(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Seat> { new Seat { Id = 1 } });
            _eventSeatRepository.Setup(x => x.GetAllByEvent(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventSeat> { new EventSeat { EventId = 1, SeatId = 1 } });

            var result = await _service.LinkAllSeatsToEvent(1, CancellationToken.None);

            Assert.Equal(OrderResultStatus.Success, result.Status);
            Assert.Equal(0, result.LinkedCount);
            _eventSeatRepository.Verify(
                x => x.Add(It.IsAny<EventSeat>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _eventSeatRepository.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}