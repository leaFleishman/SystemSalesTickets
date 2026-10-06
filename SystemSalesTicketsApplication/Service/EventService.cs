using AutoMapper;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using Microsoft.Extensions.Logging;

namespace SystemSalesTickets.Service.Service
{
    public class EventService : IEventService
    {
        private const int NameMinLength = 3;
        private const int NameMaxLength = 50;

        private readonly ILogger<EventService> _logger;
        private readonly ISeatRepository _seatRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventSeatRepository _eventSeatRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IEventCancellationNotifier _cancellationNotifier;
        private readonly IMapper _mapper;

        public EventService(IEventRepository eventRepo, IMapper mapper, ILogger<EventService> logger, ISeatRepository seatRepository, IEventSeatRepository eventSeatRepository, IOrderRepository orderRepository, IEventCancellationNotifier cancellationNotifier)
        {
            _eventRepository = eventRepo;
            _mapper = mapper;
            _logger = logger;
            _seatRepository = seatRepository;
            _eventSeatRepository = eventSeatRepository;
            _orderRepository = orderRepository;
            _cancellationNotifier = cancellationNotifier;
        }

        public async Task<EventDTO> Add(EventDTO e, CancellationToken cancellationToken = default)
        {
            var tmp = _mapper.Map<Event>(e);
            tmp.IsCancelled = false;
            tmp.CancelledAt = null;
            tmp.CancellationReason = null;

            await _eventRepository.Add(tmp, cancellationToken);
            var seats = await _seatRepository.GetAllSeats(cancellationToken);

            foreach (var seat in seats)
            {
                await _eventSeatRepository.Add(new EventSeat
                {
                    Event = tmp,
                    SeatId = seat.Id,
                    IsAvailable = true
                }, cancellationToken);
            }

            await _eventRepository.Save(cancellationToken);
            return _mapper.Map<EventDTO>(tmp);
        }

        public async Task<PagedResponse<EventDTO>> GetAll(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            // Customers must never receive events that have already started.
            // Managers can still use the existing repository methods for administration.
            var result = await _eventRepository.GetUpcomingAsync(
                DateTime.UtcNow,
                pageNumber,
                pageSize,
                cancellationToken);

            return new PagedResponse<EventDTO>(
                _mapper.Map<IEnumerable<EventDTO>>(result.Data),
                result.PageNumber,
                result.PageSize,
                result.TotalRecords);
        }

        public async Task<EventDTO> GetEventByName(string name, CancellationToken cancellationToken = default)
        {
            var tmp = await _eventRepository.GetEventByName(name, cancellationToken);
            return _mapper.Map<EventDTO>(tmp);
        }

        public async Task<EventResultDTO> Update(int id, UpdateEventDTO dto, CancellationToken cancellationToken = default)
        {
            var ev = await _eventRepository.GetById(id, cancellationToken);
            if (ev == null) return Fail(EventResultStatus.NotFound, "Event not found");
            if (ev.IsCancelled) return Fail(EventResultStatus.Conflict, "A cancelled event cannot be edited");
            if (ToUtc(ev.Date) <= DateTime.UtcNow) return Fail(EventResultStatus.Conflict, "An event that already took place cannot be edited");

            var name = (dto.Name ?? string.Empty).Trim();
            if (name.Length < NameMinLength || name.Length > NameMaxLength)
                return Fail(EventResultStatus.Invalid, $"Name must be between {NameMinLength} and {NameMaxLength} characters");
            if (dto.Price < 0) return Fail(EventResultStatus.Invalid, "Price must be non-negative");
            if (dto.NumberOfSeats < 1) return Fail(EventResultStatus.Invalid, "Number of seats must be at least 1");

            var newDate = ToUtc(dto.Date);
            var dateChanged = newDate != ToUtc(ev.Date);
            if (dateChanged && newDate <= DateTime.UtcNow)
                return Fail(EventResultStatus.Invalid, "The event date must be in the future");

            var ordersCount = await _orderRepository.CountByEvent(id, cancellationToken);
            if (dto.NumberOfSeats < ordersCount)
                return Fail(EventResultStatus.Conflict, $"Cannot reduce the number of seats below the {ordersCount} tickets already ordered");

            if (!string.Equals(name, ev.Name, StringComparison.Ordinal))
            {
                var sameName = await _eventRepository.GetEventByName(name, cancellationToken);
                if (sameName != null && sameName.Id != id)
                    return Fail(EventResultStatus.Conflict, "Another event already uses this name");
            }

            if (dateChanged && await _eventRepository.DateInUse(newDate, id, cancellationToken))
                return Fail(EventResultStatus.Conflict, "Another event already takes place at this date and time");

            ev.Name = name;
            ev.Date = newDate;
            ev.Price = (double)dto.Price;
            ev.NumberOfSeats = dto.NumberOfSeats;

            await _eventRepository.Update(ev, cancellationToken);
            await _eventRepository.Save(cancellationToken);

            _logger.LogInformation("Event {EventId} updated", id);
            return new EventResultDTO { Status = EventResultStatus.Success, Event = _mapper.Map<EventDTO>(ev) };
        }

        public async Task<EventResultDTO> Cancel(int id, string? reason, CancellationToken cancellationToken = default)
        {
            var ev = await _eventRepository.GetById(id, cancellationToken);
            if (ev == null) return Fail(EventResultStatus.NotFound, "Event not found");
            if (ev.IsCancelled) return Fail(EventResultStatus.Conflict, "The event is already cancelled");
            if (ToUtc(ev.Date) <= DateTime.UtcNow) return Fail(EventResultStatus.Conflict, "An event that already took place cannot be cancelled");

            ev.IsCancelled = true;
            ev.CancelledAt = DateTime.UtcNow;
            ev.CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

            await _eventRepository.Update(ev, cancellationToken);
            await _eventRepository.Save(cancellationToken);

            var orders = await _orderRepository.GetOrdersByEvent(id, cancellationToken);
            var notified = 0;

            if (orders.Count > 0)
            {
                try
                {
                    notified = await _cancellationNotifier.NotifyAsync(ev, orders, ev.CancellationReason, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to notify customers about cancelled event {EventId}", id);
                }
            }

            return new EventResultDTO
            {
                Status = EventResultStatus.Success,
                Event = _mapper.Map<EventDTO>(ev),
                AffectedOrders = orders.Count,
                NotificationsSent = notified
            };
        }

        private static EventResultDTO Fail(EventResultStatus status, string message)
            => new EventResultDTO { Status = status, Message = message };

        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
