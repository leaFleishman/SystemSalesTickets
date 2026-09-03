using Microsoft.AspNetCore.Mvc;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("GetEvents")]
        public async Task<IEnumerable<EventDTO>> GetEvents()
        {
            return await _eventService.GetAll();
        }

        [HttpPost("AddEvent")]
        public async Task<ActionResult<EventDTO>> AddEvent([FromBody] EventDTO e)
        {
            await _eventService.Add(e);
            return Created();
        }


    }
}
