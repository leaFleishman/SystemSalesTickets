
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;
using MyApp.Application.Common.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SystemSalesTickets.Tests
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILogger<EventService>> _logger;

        private readonly EventService _service;

        public EventServiceTests()
        {
            _eventRepository = new Mock<IEventRepository>();
            _mapper = new Mock<IMapper>();
            _logger = new Mock<ILogger<EventService>>();

            //_service = new EventService(
            //    _eventRepository.Object,
            //    _mapper.Object,
            //    _logger.Object);
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
                Date =  new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            };

            _mapper
                .Setup(x => x.Map<Event>(eventDto))
                .Returns(eventModel);

            _mapper
                .Setup(x => x.Map<EventDTO>(eventModel))
                .Returns(resultDto);

            // Act
            var result = await _service.Add(eventDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Concert", result.Name);
            Assert.Equal(100, result.Price);
            Assert.Equal(50, result.NumberOfSeats);

            _eventRepository.Verify(
                x => x.Add(eventModel, It.IsAny<CancellationToken>()),
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
                    EventId = 1,
                    Name = "Concert",
                    Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Price = 100,
                    NumberOfSeats = 50
                },

                new Event
                {
                    EventId = 2,
                    Name = "Show",
                    Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Price = 200,
                    NumberOfSeats = 100
                }
            };

            var eventDtos = new List<EventDTO>
            {
                new EventDTO
                {
                    Name = "Concert",
                    Date =new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Price = 100,
                    NumberOfSeats = 50
                },

                new EventDTO
                {
                    Name = "Show",
                    Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Price = 200,
                    NumberOfSeats = 100
                }
            };

            _eventRepository
                .Setup(x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResponse<Event>(events, 1, 20, events.Count));

            _mapper
                .Setup(x => x.Map<IEnumerable<EventDTO>>(events))
                .Returns(eventDtos);

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());

            _eventRepository.Verify(
                x => x.GetAllAsync(1, 20, It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task GetEventByName_ShouldReturnEvent()
        {
            // Arrange
            string name = "Concert";

            var eventModel = new Event
            {
                EventId = 1,
                Name = "Concert",
                Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            };

            var eventDto = new EventDTO
            {
                Name = "Concert",
                Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 100,
                NumberOfSeats = 50
            };

            _eventRepository
                .Setup(x => x.GetEventByName(name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventModel);

            _mapper
                .Setup(x => x.Map<EventDTO>(eventModel))
                .Returns(eventDto);

            // Act
            var result = await _service.GetEventByName(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Concert", result.Name);

            _eventRepository.Verify(
                x => x.GetEventByName(name, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

