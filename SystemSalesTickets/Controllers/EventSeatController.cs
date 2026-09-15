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

        
        [HttpGet("event/{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> GetSeatsForEvent([FromRoute] int eventId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetSeatsForEvent request received for EventId {EventId}", eventId);

            var seats = await _eventSeatService.GetSeatsForEvent(eventId, cancellationToken);

            return Ok(seats);
        }

        
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

        // Links every seat that isn't already linked to this event, in one go.
        [HttpPost("event/{eventId:int}/link-all")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> LinkAllSeatsToEvent([FromRoute] int eventId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("LinkAllSeatsToEvent request received for EventId {EventId}", eventId);

            var result = await _eventSeatService.LinkAllSeatsToEvent(eventId, cancellationToken);

            return result.Status switch
            {
                OrderResultStatus.NotFound => NotFound(result.Message),
                OrderResultStatus.Success => Ok(new { linkedCount = result.LinkedCount }),
                _ => BadRequest()
            };
        }
    }
}
