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
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILogger<EventService>> _logger;
        private readonly Mock<ISeatRepository> _seatRepository;
        private readonly Mock<IEventSeatRepository> _eventSeatRepository;
        private readonly Mock<IOrderRepository> _orderRepository;
        private readonly Mock<IEventCancellationNotifier> _notifier;
        private readonly EventService _service;

        public EventServiceTests()
        {
            _eventRepository = new Mock<IEventRepository>();
            _mapper = new Mock<IMapper>();
            _logger = new Mock<ILogger<EventService>>();
            _seatRepository = new Mock<ISeatRepository>();
            _eventSeatRepository = new Mock<IEventSeatRepository>();
            _orderRepository = new Mock<IOrderRepository>();
            _notifier = new Mock<IEventCancellationNotifier>();

            _service = new EventService(
                _eventRepository.Object,
                _mapper.Object,
                _logger.Object,
                _seatRepository.Object,
                _eventSeatRepository.Object,
                _orderRepository.Object,
                _notifier.Object);
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
        public async Task Add_WhenRepositoryThrows_DoesNotContinueToCreateSeats()
        {
            var eventDto = new EventDTO
            {
                Name = "Concert",
                Date = new DateTime(2026, 1, 1),
                Price = 100,
                NumberOfSeats = 2
            };

            var eventModel = new Event { Name = "Concert" };

            _mapper
                .Setup(x => x.Map<Event>(eventDto))
                .Returns(eventModel);

            _eventRepository
                .Setup(x => x.Add(eventModel, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(
                () => _service.Add(eventDto, CancellationToken.None));

            _seatRepository.Verify(
                x => x.GetAllSeats(It.IsAny<CancellationToken>()),
                Times.Never);
            _eventRepository.Verify(
                x => x.Save(It.IsAny<CancellationToken>()),
                Times.Never);
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

        // ---------------------------------------------------------------
        // Helpers for the edit / cancel tests
        // ---------------------------------------------------------------

        private static Event ActiveEvent(int id = 1) => new Event
        {
            Id = id,
            Name = "Concert",
            Date = DateTime.UtcNow.AddDays(30),
            Price = 100,
            NumberOfSeats = 100
        };

        private static UpdateEventDTO ValidUpdate(Event ev) => new UpdateEventDTO
        {
            Name = ev.Name,
            Date = ev.Date,
            Price = 150,
            NumberOfSeats = ev.NumberOfSeats
        };

        private void SetupFind(int id, Event? ev)
        {
            _eventRepository
                .Setup(x => x.GetById(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ev!);
        }

        private void SetupMapperToDto()
        {
            _mapper
                .Setup(x => x.Map<EventDTO>(It.IsAny<object>()))
                .Returns((object source) =>
                {
                    var e = (Event)source;
                    return new EventDTO
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Date = e.Date,
                        Price = (decimal)e.Price,
                        NumberOfSeats = e.NumberOfSeats,
                        IsCancelled = e.IsCancelled,
                        CancelledAt = e.CancelledAt,
                        CancellationReason = e.CancellationReason
                    };
                });
        }

        private void VerifyNothingSaved()
        {
            _eventRepository.Verify(x => x.Save(It.IsAny<CancellationToken>()), Times.Never);
        }

        // ---------------------------------------------------------------
        // Add
        // ---------------------------------------------------------------

        [Fact]
        public async Task Add_IgnoresCancellationFieldsSentByClient()
        {
            var eventDto = new EventDTO
            {
                Name = "Concert",
                Date = DateTime.UtcNow.AddDays(10),
                Price = 100,
                NumberOfSeats = 2,
                IsCancelled = true
            };

            var eventModel = new Event
            {
                Name = "Concert",
                Date = eventDto.Date,
                IsCancelled = true,
                CancelledAt = DateTime.UtcNow,
                CancellationReason = "hack"
            };

            _mapper.Setup(x => x.Map<Event>(eventDto)).Returns(eventModel);
            _eventRepository
                .Setup(x => x.Add(eventModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventModel);
            _seatRepository
                .Setup(x => x.GetAllSeats(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Seat>());
            _mapper.Setup(x => x.Map<EventDTO>(eventModel)).Returns(eventDto);

            await _service.Add(eventDto, CancellationToken.None);

            Assert.False(eventModel.IsCancelled);
            Assert.Null(eventModel.CancelledAt);
            Assert.Null(eventModel.CancellationReason);
        }

        // ---------------------------------------------------------------
        // Update
        // ---------------------------------------------------------------

        [Fact]
        public async Task Update_WhenEventDoesNotExist_ReturnsNotFound()
        {
            SetupFind(7, null);

            var result = await _service.Update(7, new UpdateEventDTO { Name = "Concert" });

            Assert.Equal(EventResultStatus.NotFound, result.Status);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WhenEventIsCancelled_ReturnsConflict()
        {
            var ev = ActiveEvent();
            ev.IsCancelled = true;
            SetupFind(1, ev);

            var result = await _service.Update(1, ValidUpdate(ev));

            Assert.Equal(EventResultStatus.Conflict, result.Status);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WhenEventAlreadyTookPlace_ReturnsConflict()
        {
            var ev = ActiveEvent();
            ev.Date = DateTime.UtcNow.AddDays(-1);
            SetupFind(1, ev);

            var result = await _service.Update(1, ValidUpdate(ev));

            Assert.Equal(EventResultStatus.Conflict, result.Status);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WithValidData_UpdatesFieldsAndSaves()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            SetupMapperToDto();
            var newDate = DateTime.UtcNow.AddDays(40);

            var result = await _service.Update(1, new UpdateEventDTO
            {
                Name = "  New name  ",
                Date = newDate,
                Price = 150.5m,
                NumberOfSeats = 250
            });

            Assert.Equal(EventResultStatus.Success, result.Status);
            Assert.Equal("New name", ev.Name);
            Assert.Equal(newDate, ev.Date);
            Assert.Equal(150.5, ev.Price);
            Assert.Equal(250, ev.NumberOfSeats);
            Assert.NotNull(result.Event);
            Assert.Equal("New name", result.Event!.Name);

            _eventRepository.Verify(x => x.Update(ev, It.IsAny<CancellationToken>()), Times.Once);
            _eventRepository.Verify(x => x.Save(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ab  ")]
        public async Task Update_WithTooShortName_ReturnsInvalid(string name)
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            var dto = ValidUpdate(ev);
            dto.Name = name;

            var result = await _service.Update(1, dto);

            Assert.Equal(EventResultStatus.Invalid, result.Status);
            Assert.Equal("Concert", ev.Name);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WithNegativePrice_ReturnsInvalid()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            var dto = ValidUpdate(ev);
            dto.Price = -1;

            var result = await _service.Update(1, dto);

            Assert.Equal(EventResultStatus.Invalid, result.Status);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WithNewDateInThePast_ReturnsInvalid()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            var dto = ValidUpdate(ev);
            dto.Date = DateTime.UtcNow.AddDays(-2);

            var result = await _service.Update(1, dto);

            Assert.Equal(EventResultStatus.Invalid, result.Status);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WhenReducingSeatsBelowOrderedTickets_ReturnsConflict()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            _orderRepository
                .Setup(x => x.CountByEvent(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);
            var dto = ValidUpdate(ev);
            dto.NumberOfSeats = 4;

            var result = await _service.Update(1, dto);

            Assert.Equal(EventResultStatus.Conflict, result.Status);
            Assert.Equal(100, ev.NumberOfSeats);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WhenSeatsEqualOrderedTickets_IsAllowed()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            SetupMapperToDto();
            _orderRepository
                .Setup(x => x.CountByEvent(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);
            var dto = ValidUpdate(ev);
            dto.NumberOfSeats = 5;

            var result = await _service.Update(1, dto);

            Assert.Equal(EventResultStatus.Success, result.Status);
            Assert.Equal(5, ev.NumberOfSeats);
        }

        [Fact]
        public async Task Update_WhenNameBelongsToAnotherEvent_ReturnsConflict()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            _eventRepository
                .Setup(x => x.GetEventByName("Taken", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event { Id = 2, Name = "Taken" });
            var dto = ValidUpdate(ev);
            dto.Name = "Taken";

            var result = await _service.Update(1, dto);

            Assert.Equal(EventResultStatus.Conflict, result.Status);
            Assert.Equal("Concert", ev.Name);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WhenNameUnchanged_DoesNotCheckNameUniqueness()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            SetupMapperToDto();

            var result = await _service.Update(1, ValidUpdate(ev));

            Assert.Equal(EventResultStatus.Success, result.Status);
            _eventRepository.Verify(
                x => x.GetEventByName(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Update_WhenNewDateIsUsedByAnotherEvent_ReturnsConflict()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            var dto = ValidUpdate(ev);
            dto.Date = DateTime.UtcNow.AddDays(50);
            _eventRepository
                .Setup(x => x.DateInUse(dto.Date, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.Update(1, dto);

            Assert.Equal(EventResultStatus.Conflict, result.Status);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Update_WhenDateUnchanged_DoesNotCheckDateUniqueness()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            SetupMapperToDto();

            var result = await _service.Update(1, ValidUpdate(ev));

            Assert.Equal(EventResultStatus.Success, result.Status);
            _eventRepository.Verify(
                x => x.DateInUse(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ---------------------------------------------------------------
        // Cancel
        // ---------------------------------------------------------------

        [Fact]
        public async Task Cancel_WhenEventDoesNotExist_ReturnsNotFound()
        {
            SetupFind(7, null);

            var result = await _service.Cancel(7, "reason");

            Assert.Equal(EventResultStatus.NotFound, result.Status);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Cancel_WhenAlreadyCancelled_ReturnsConflict()
        {
            var ev = ActiveEvent();
            ev.IsCancelled = true;
            SetupFind(1, ev);

            var result = await _service.Cancel(1, "reason");

            Assert.Equal(EventResultStatus.Conflict, result.Status);
            VerifyNothingSaved();
            _notifier.Verify(
                x => x.NotifyAsync(It.IsAny<Event>(), It.IsAny<IReadOnlyCollection<Order>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Cancel_WhenEventAlreadyTookPlace_ReturnsConflict()
        {
            var ev = ActiveEvent();
            ev.Date = DateTime.UtcNow.AddHours(-3);
            SetupFind(1, ev);

            var result = await _service.Cancel(1, null);

            Assert.Equal(EventResultStatus.Conflict, result.Status);
            Assert.False(ev.IsCancelled);
            VerifyNothingSaved();
        }

        [Fact]
        public async Task Cancel_MarksEventCancelled_SavesAndNotifiesTicketHolders()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            SetupMapperToDto();
            var orders = new List<Order>
            {
                new Order { Id = 1, UserId = 2, EventId = 1 },
                new Order { Id = 2, UserId = 3, EventId = 1 },
                new Order { Id = 3, UserId = 3, EventId = 1 }
            };
            _orderRepository
                .Setup(x => x.GetOrdersByEvent(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(orders);
            _notifier
                .Setup(x => x.NotifyAsync(ev, orders, "Artist is ill", It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            var before = DateTime.UtcNow;
            var result = await _service.Cancel(1, "  Artist is ill  ");

            Assert.Equal(EventResultStatus.Success, result.Status);
            Assert.True(ev.IsCancelled);
            Assert.Equal("Artist is ill", ev.CancellationReason);
            Assert.NotNull(ev.CancelledAt);
            Assert.True(ev.CancelledAt >= before);
            Assert.Equal(3, result.AffectedOrders);
            Assert.Equal(2, result.NotificationsSent);
            Assert.True(result.Event!.IsCancelled);

            _eventRepository.Verify(x => x.Save(It.IsAny<CancellationToken>()), Times.Once);
            _notifier.Verify(
                x => x.NotifyAsync(ev, orders, "Artist is ill", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Cancel_WithBlankReason_StoresNullReason()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            SetupMapperToDto();

            var result = await _service.Cancel(1, "   ");

            Assert.Equal(EventResultStatus.Success, result.Status);
            Assert.Null(ev.CancellationReason);
        }

        [Fact]
        public async Task Cancel_WhenNobodyBoughtTickets_DoesNotSendNotifications()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            SetupMapperToDto();
            _orderRepository
                .Setup(x => x.GetOrdersByEvent(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Order>());

            var result = await _service.Cancel(1, "reason");

            Assert.Equal(EventResultStatus.Success, result.Status);
            Assert.Equal(0, result.AffectedOrders);
            Assert.Equal(0, result.NotificationsSent);
            _notifier.Verify(
                x => x.NotifyAsync(It.IsAny<Event>(), It.IsAny<IReadOnlyCollection<Order>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Cancel_WhenNotificationFails_StillSucceeds()
        {
            var ev = ActiveEvent();
            SetupFind(1, ev);
            SetupMapperToDto();
            _orderRepository
                .Setup(x => x.GetOrdersByEvent(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Order> { new Order { Id = 1, UserId = 2, EventId = 1 } });
            _notifier
                .Setup(x => x.NotifyAsync(It.IsAny<Event>(), It.IsAny<IReadOnlyCollection<Order>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mail server down"));

            var result = await _service.Cancel(1, "reason");

            Assert.Equal(EventResultStatus.Success, result.Status);
            Assert.True(ev.IsCancelled);
            Assert.Equal(1, result.AffectedOrders);
            Assert.Equal(0, result.NotificationsSent);
            _eventRepository.Verify(x => x.Save(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
