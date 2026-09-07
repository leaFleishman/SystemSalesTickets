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
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEvents(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetEvents request received");

            var events = await _eventService.GetAll(cancellationToken);

            _logger.LogInformation("GetEvents returned {Count} events", events?.Count() ?? 0);

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

        [HttpGet("by-name/{name}")]
        [Authorize]
        public async Task<ActionResult<EventDTO>> GetEventByName(string name, CancellationToken cancellationToken)
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
    }
}