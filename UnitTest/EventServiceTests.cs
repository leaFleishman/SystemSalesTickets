using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;

namespace SystemSalesTickets.Tests
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILogger<EventService>> _logger;
        private readonly Mock<ISeatRepository> _seatRepository;
        private readonly Mock<IEventSeatRepository> _eventSeatRepository;
        private readonly EventService _service;

    public EventServiceTests()
        {
            _eventRepository = new Mock<IEventRepository>();
            _mapper = new Mock<IMapper>();
            _logger = new Mock<ILogger<EventService>>();
            _seatRepository = new Mock<ISeatRepository>();
            _eventSeatRepository = new Mock<IEventSeatRepository>();

            _service = new EventService(
                _eventRepository.Object,
                _mapper.Object,
                _logger.Object,
                _seatRepository.Object,
                _eventSeatRepository.Object);
        }

        [Fact]
        public async Task AddEvent_ShouldAddEventAndReturnDto()
        {
            // Arrange
            var eventDto = new EventDTO
            {
                Name = "Concert",
                Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            };

            var eventModel = new Event
            {
                Name = "Concert",
                Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            };

            var resultDto = new EventDTO
            {
                Name = "Concert",
                Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            };

            var seats = new List<Seat>
        {
            new Seat { Id = 1, Row = 1, Line = 1 },
            new Seat { Id = 2, Row = 1, Line = 2 }
        };

            _mapper
                .Setup(x => x.Map<Event>(eventDto))
                .Returns(eventModel);

            _mapper
                .Setup(x => x.Map<EventDTO>(eventModel))
                .Returns(resultDto);

            _seatRepository
                .Setup(x => x.GetAllAsync(
                    1,
                    int.MaxValue,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PagedResponse<Seat>(
                        seats,
                        1,
                        int.MaxValue,
                        seats.Count));

            _eventRepository
                .Setup(x => x.Add(
                    eventModel,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventModel);

            _eventRepository
                .Setup(x => x.Save(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _eventSeatRepository
                .Setup(x => x.Add(
                    It.IsAny<EventSeat>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (EventSeat eventSeat, CancellationToken _) => eventSeat);

            // Act
            var result = await _service.Add(
                eventDto,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Concert", result.Name);
            Assert.Equal(100, result.Price);
            Assert.Equal(50, result.NumberOfSeats);

            _eventRepository.Verify(
                x => x.Add(
                    eventModel,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _seatRepository.Verify(
                x => x.GetAllAsync(
                    1,
                    int.MaxValue,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _eventSeatRepository.Verify(
                x => x.Add(
                    It.IsAny<EventSeat>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(seats.Count));

            _eventRepository.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllEvents()
        {
            // Arrange
            var events = new List<Event>
        {
            new Event
            {
                Id = 1,
                Name = "Concert",
                Date = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            },

            new Event
            {
                Id = 2,
                Name = "Show",
                Date = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 200,
                NumberOfSeats = 100
            }
        };

            var eventDtos = new List<EventDTO>
        {
            new EventDTO
            {
                Name = "Concert",
                Date = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            },

            new EventDTO
            {
                Name = "Show",
                Date = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 200,
                NumberOfSeats = 100
            }
        };

            _eventRepository
                .Setup(x => x.GetAllAsync(
                    1,
                    20,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PagedResponse<Event>(
                        events,
                        1,
                        20,
                        events.Count));

            _mapper
                .Setup(x => x.Map<IEnumerable<EventDTO>>(events))
                .Returns(eventDtos);

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());

            _eventRepository.Verify(
                x => x.GetAllAsync(
                    1,
                    20,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetEventByName_ShouldReturnEvent()
        {
            // Arrange
            string name = "Concert";

            var eventModel = new Event
            {
                Id = 1,
                Name = "Concert",
                Date = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            };

            var eventDto = new EventDTO
            {
                Name = "Concert",
                Date = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            };

            _eventRepository
                .Setup(x => x.GetEventByName(
                    name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventModel);

            _mapper
                .Setup(x => x.Map<EventDTO>(eventModel))
                .Returns(eventDto);

            // Act
            var result = await _service.GetEventByName(
                name,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Concert", result.Name);

            _eventRepository.Verify(
                x => x.GetEventByName(
                    name,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

}
