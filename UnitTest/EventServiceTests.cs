using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;

namespace UnitTest
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
            var eventDto = new EventDTO
            {
                Name = "Concert",
                Date = new DateTime(2026, 1, 1),
                Price = 100,
                NumberOfSeats = 2
            };

            var eventModel = new Event
            {
                Name = "Concert",
                Date = eventDto.Date,
                Price = 100,
                NumberOfSeats = 2
            };

            var seats = new List<Seat>
            {
                new Seat { Id = 1, Row = 1, Line = 1 },
                new Seat { Id = 2, Row = 1, Line = 2 }
            };

            _mapper
                .Setup(x => x.Map<Event>(eventDto))
                .Returns(eventModel);

            _eventRepository
                .Setup(x => x.Add(eventModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventModel);

            _seatRepository
                .Setup(x => x.GetAllSeats(It.IsAny<CancellationToken>()))
                .ReturnsAsync(seats);

            _eventSeatRepository
                .Setup(x => x.Add(
                    It.IsAny<EventSeat>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventSeat eventSeat, CancellationToken _) => eventSeat);

            _eventRepository
                .Setup(x => x.Save(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<EventDTO>(eventModel))
                .Returns(eventDto);

            var result = await _service.Add(
                eventDto,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(eventDto.Name, result.Name);
            Assert.Equal(eventDto.Price, result.Price);

            _eventRepository.Verify(
                x => x.Add(
                    eventModel,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _seatRepository.Verify(
                x => x.GetAllSeats(
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
        public async Task AddEvent_CreatesAvailableEventSeatsForAllSeats()
        {
            var eventDto = new EventDTO
            {
                Name = "Concert",
                Date = new DateTime(2026, 1, 1),
                Price = 100,
                NumberOfSeats = 2
            };

            var eventModel = new Event
            {
                Name = "Concert",
                Date = eventDto.Date,
                Price = 100,
                NumberOfSeats = 2
            };

            var seats = new List<Seat>
            {
                new Seat { Id = 1, Row = 1, Line = 1 },
                new Seat { Id = 2, Row = 1, Line = 2 }
            };

            _mapper
                .Setup(x => x.Map<Event>(eventDto))
                .Returns(eventModel);

            _eventRepository
                .Setup(x => x.Add(
                    eventModel,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventModel);

            _seatRepository
                .Setup(x => x.GetAllSeats(It.IsAny<CancellationToken>()))
                .ReturnsAsync(seats);

            _eventSeatRepository
                .Setup(x => x.Add(
                    It.IsAny<EventSeat>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventSeat eventSeat, CancellationToken _) => eventSeat);

            _eventRepository
                .Setup(x => x.Save(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<EventDTO>(eventModel))
                .Returns(eventDto);

            await _service.Add(
                eventDto,
                CancellationToken.None);

            _eventSeatRepository.Verify(
                x => x.Add(
                    It.Is<EventSeat>(es =>
                        es.Event == eventModel &&
                        es.IsAvailable &&
                        (es.SeatId == 1 || es.SeatId == 2)),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(seats.Count));
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllEvents()
        {
            var events = new List<Event>
            {
                new Event
                {
                    Name = "Concert 1",
                    Price = 100
                },
                new Event
                {
                    Name = "Concert 2",
                    Price = 200
                }
            };

            var eventDtos = new List<EventDTO>
            {
                new EventDTO
                {
                    Name = "Concert 1",
                    Price = 100
                },
                new EventDTO
                {
                    Name = "Concert 2",
                    Price = 200
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

            var result = await _service.GetAll();

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
            const string name = "Concert";

            var eventModel = new Event
            {
                Name = name,
                Price = 100
            };

            var eventDto = new EventDTO
            {
                Name = name,
                Price = 100
            };

            _eventRepository
                .Setup(x => x.GetEventByName(
                    name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventModel);

            _mapper
                .Setup(x => x.Map<EventDTO>(eventModel))
                .Returns(eventDto);

            var result = await _service.GetEventByName(
                name,
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(100, result.Price);

            _eventRepository.Verify(
                x => x.GetEventByName(
                    name,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetEventByName_WhenEventDoesNotExist_ReturnsNull()
        {
            const string name = "NotFound";

            _eventRepository
                .Setup(x => x.GetEventByName(
                    name,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event)null!);

            _mapper
                .Setup(x => x.Map<EventDTO>(null))
                .Returns((EventDTO)null!);

            var result = await _service.GetEventByName(
                name,
                CancellationToken.None);

            Assert.Null(result);

            _eventRepository.Verify(
                x => x.GetEventByName(
                    name,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}