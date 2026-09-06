using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        readonly IEventService _eventService;
        private readonly ILogger<EventController> _logger;

        public EventController(IEventService eventService, ILogger<EventController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpGet("GetEvents")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IEnumerable<EventDTO>> GetEvents()
        {
            _logger.LogInformation("GetEvents request received");

            var events = await _eventService.GetAll();

            _logger.LogInformation("GetEvents returned {Count} events", events?.Count() ?? 0);

            return events;
        }

        [HttpPost("AddEvent")]
        [Authorize]
        public async Task<ActionResult<EventDTO>> AddEvent([FromBody] EventDTO e)
        {
            _logger.LogInformation("AddEvent request received for event {EventName}", e?.Name);
            await _eventService.Add(e);
            _logger.LogInformation("AddEvent completed successfully for event {EventName}", e?.Name);
            return Created();

        }

        [HttpGet("GetEventByName")]
        [Authorize]
        public async Task<ActionResult<EventDTO>> GetEventByName(string name)
        {

        return   await _eventService.GetEventByName(name);
        }
    }
}