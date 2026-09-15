using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Service.Service
{
    public class EventSeatService : IEventSeatService
    {
        private readonly IEventSeatRepository _eventSeatRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly ILogger<EventSeatService> _logger;

        public EventSeatService(
            IEventSeatRepository eventSeatRepository,
            IEventRepository eventRepository,
            ISeatRepository seatRepository,
            ILogger<EventSeatService> logger)
        {
            _eventSeatRepository = eventSeatRepository;
            _eventRepository = eventRepository;
            _seatRepository = seatRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<EventSeatDTO>> GetSeatsForEvent(int eventId, CancellationToken cancellationToken = default)
        {
            var eventSeats = await _eventSeatRepository.GetAllByEvent(eventId, cancellationToken);

            return eventSeats.Select(es => new EventSeatDTO
            {
                EventId = es.EventId,
                SeatId = es.SeatId,
                Row = es.Seat.Row,
                Line = es.Seat.Line,
                IsAvailable = es.IsAvailable,
                Version = es.Version
            });
        }

        public async Task<EventSeatResultDTO> AddEventSeat(AddEventSeatDTO dto, CancellationToken cancellationToken = default)
        {
            var ev = await _eventRepository.GetById(dto.EventId, cancellationToken);
            if (ev == null)
            {
                _logger.LogWarning("AddEventSeat failed: Event {EventId} not found", dto.EventId);
                return new EventSeatResultDTO { Status = OrderResultStatus.NotFound, Message = "Event not found" };
            }

            var seat = await _seatRepository.GetById(dto.SeatId, cancellationToken);
            if (seat == null)
            {
                _logger.LogWarning("AddEventSeat failed: Seat {SeatId} not found", dto.SeatId);
                return new EventSeatResultDTO { Status = OrderResultStatus.NotFound, Message = "Seat not found" };
            }

            var existing = await _eventSeatRepository.GetByEventAndSeat(dto.EventId, dto.SeatId, cancellationToken);
            if (existing != null)
            {
                _logger.LogWarning("AddEventSeat conflict: Seat {SeatId} already linked to Event {EventId}", dto.SeatId, dto.EventId);
                return new EventSeatResultDTO { Status = OrderResultStatus.Conflict, Message = "This seat is already linked to the event" };
            }

            var eventSeat = new EventSeat
            {
                EventId = dto.EventId,
                SeatId = dto.SeatId,
                IsAvailable = true
            };

            await _eventSeatRepository.Add(eventSeat, cancellationToken);
            await _eventSeatRepository.Save(cancellationToken);

            _logger.LogInformation("Seat {SeatId} linked to Event {EventId}", dto.SeatId, dto.EventId);

            return new EventSeatResultDTO
            {
                Status = OrderResultStatus.Success,
                EventSeat = new EventSeatDTO
                {
                    EventId = eventSeat.EventId,
                    SeatId = eventSeat.SeatId,
                    Row = seat.Row,
                    Line = seat.Line,
                    IsAvailable = eventSeat.IsAvailable,
                    Version = eventSeat.Version
                }
            };
        }
        public async Task<LinkAllSeatsResultDTO> LinkAllSeatsToEvent(int eventId, CancellationToken cancellationToken = default)
        {
            var ev = await _eventRepository.GetById(eventId, cancellationToken);
            if (ev == null)
            {
                _logger.LogWarning("LinkAllSeatsToEvent failed: Event {EventId} not found", eventId);
                return new LinkAllSeatsResultDTO { Status = OrderResultStatus.NotFound, Message = "Event not found" };
            }

            var allSeats = await _seatRepository.GetAllSeats(cancellationToken);
            var existingLinks = await _eventSeatRepository.GetAllByEvent(eventId, cancellationToken);
            var alreadyLinkedSeatIds = existingLinks.Select(es => es.SeatId).ToHashSet();

            var seatsToLink = allSeats.Where(s => !alreadyLinkedSeatIds.Contains(s.Id)).ToList();

            foreach (var seat in seatsToLink)
            {
                await _eventSeatRepository.Add(
                    new EventSeat
                    {
                        EventId = eventId,
                        SeatId = seat.Id,
                        IsAvailable = true
                    },
                    cancellationToken);
            }

            if (seatsToLink.Count > 0)
            {
                await _eventSeatRepository.Save(cancellationToken);
            }

            _logger.LogInformation("Linked {Count} seats to Event {EventId}", seatsToLink.Count, eventId);

            return new LinkAllSeatsResultDTO
            {
                Status = OrderResultStatus.Success,
                LinkedCount = seatsToLink.Count
            };
        }
    }
}
