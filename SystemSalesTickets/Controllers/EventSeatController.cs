using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventSeatController : ControllerBase
    {
        private readonly IEventSeatService _eventSeatService;
        private readonly ILogger<EventSeatController> _logger;

        public EventSeatController(IEventSeatService eventSeatService, ILogger<EventSeatController> logger)
        {
            _eventSeatService = eventSeatService;
            _logger = logger;
        }

        // Any authenticated user needs this to see which seats are actually
        // bookable for a given event — this is what the seat picker on the
        // event detail page calls.
        [HttpGet("event/{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> GetSeatsForEvent([FromRoute] int eventId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetSeatsForEvent request received for EventId {EventId}", eventId);

            var seats = await _eventSeatService.GetSeatsForEvent(eventId, cancellationToken);

            return Ok(seats);
        }

        // Links an existing seat to an existing event. Needed because seats
        // created after an event already exists are never auto-linked to it
        // (only seats that exist at the moment an event is created get
        // linked automatically, in EventService.Add).
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> AddEventSeat([FromBody] AddEventSeatDTO dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("AddEventSeat request received for EventId {EventId}, SeatId {SeatId}", dto.EventId, dto.SeatId);

            var result = await _eventSeatService.AddEventSeat(dto, cancellationToken);

            return result.Status switch
            {
                OrderResultStatus.NotFound => NotFound(result.Message),
                OrderResultStatus.Conflict => Conflict(result.Message),
                OrderResultStatus.Success => Ok(result.EventSeat),
                _ => BadRequest()
            };
        }
    }
}
