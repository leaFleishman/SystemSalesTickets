using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventController> _logger;

        public EventController(IEventService eventService, ILogger<EventController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetEvents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetEvents request received");

            var events = await _eventService.GetAll(pageNumber, pageSize, cancellationToken);

            _logger.LogInformation("GetEvents returned page {PageNumber} with {Count} events", events.PageNumber, events.Data.Count());

            return Ok(events);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<EventDTO>> AddEvent([FromBody] EventDTO e, CancellationToken cancellationToken)
        {
            if (e == null)
            {
                return BadRequest("Invalid event data.");
            }

            _logger.LogInformation("AddEvent request received for event {EventName}", e.Name);

            var createdEvent = await _eventService.Add(e, cancellationToken);

            _logger.LogInformation("AddEvent completed successfully for event {EventName}", e.Name);

            return CreatedAtAction(nameof(GetEventByName), new { name = createdEvent.Name }, createdEvent);
        }

        [HttpGet("{name}")]
        [Authorize]
        public async Task<ActionResult<EventDTO>> GetEventByName([FromRoute]string name, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetEventByName request received for {EventName}", name);

            var eventDto = await _eventService.GetEventByName(name, cancellationToken);

            if (eventDto == null)
            {
                _logger.LogWarning("GetEventByName: no event found with name {EventName}", name);
                return NotFound();
            }

            return Ok(eventDto);
        }

        // Edits name / date / price / number of seats. Managers only.
        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> UpdateEvent(
            [FromRoute] int id,
            [FromBody] UpdateEventDTO dto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("UpdateEvent request received for EventId {EventId}", id);

            var result = await _eventService.Update(id, dto, cancellationToken);

            return ToActionResult(result, "UpdateEvent", id, result.Event);
        }

        // Cancels the event: no new orders are accepted and ticket holders are emailed. Managers only.
        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> CancelEvent(
            [FromRoute] int id,
            [FromBody] CancelEventDTO? dto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("CancelEvent request received for EventId {EventId}", id);

            var result = await _eventService.Cancel(id, dto?.Reason, cancellationToken);

            return ToActionResult(result, "CancelEvent", id, result);
        }

        // Maps the service status to an HTTP response; failures carry a plain-text message
        // (same shape the order endpoints use), which the client already knows how to display.
        private IActionResult ToActionResult(EventResultDTO result, string action, int id, object? successBody)
        {
            switch (result.Status)
            {
                case EventResultStatus.Success:
                    _logger.LogInformation("{Action} succeeded for EventId {EventId}", action, id);
                    return Ok(successBody);

                case EventResultStatus.NotFound:
                    _logger.LogWarning("{Action} failed for EventId {EventId}: {Message}", action, id, result.Message);
                    return NotFound(result.Message);

                case EventResultStatus.Conflict:
                    _logger.LogWarning("{Action} conflict for EventId {EventId}: {Message}", action, id, result.Message);
                    return Conflict(result.Message);

                default:
                    _logger.LogWarning("{Action} rejected for EventId {EventId}: {Message}", action, id, result.Message);
                    return BadRequest(result.Message);
            }
        }
    }
}
