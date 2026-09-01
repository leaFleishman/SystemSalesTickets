using Microsoft.AspNetCore.Mvc;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatController : ControllerBase
    {

        private readonly ISeatService _seatService;

        public SeatController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seat>>> GetAll()
        {
            return Ok(await _seatService.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Seat>> GetById(int id)
        {
            var seat = await _seatService.GetById(id);

            if (seat == null)
                return NotFound();

            return Ok(seat);
        }

        [HttpPost]
        public async Task<ActionResult<Seat>> Add(Seat seat)
        {
            var result = await _seatService.Add(seat);

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<Seat>> Update(Seat seat)
        {
            var result = await _seatService.Update(seat);

            return Ok(result);
        }





    }
}
