using Microsoft.AspNetCore.Mvc;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;


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
        public async Task<IEnumerable<Event>> GetEvents()
        {
            return await _eventService.GetAll();
        }

        [HttpPost("AddEvent")]
        public async Task<ActionResult<Event>> AddEvent([FromBody] Event e)
        {
            await _eventService.Add(e);
            return Created();
        }


    }
}
