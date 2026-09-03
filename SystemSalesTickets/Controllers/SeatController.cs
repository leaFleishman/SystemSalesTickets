using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;

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
        [Authorize]

        public async Task<ActionResult<IEnumerable<SeatDTO>>> GetAll()
        {
            return Ok(await _seatService.GetAll());
        }

        [HttpGet("{id}")]
        [Authorize]

        public async Task<ActionResult<SeatDTO>> GetById(int id)
        {
            var seat = await _seatService.GetById(id);

            if (seat == null)
                return NotFound();

            return Ok(seat);
        }

        [HttpPost]
        [Authorize]

        public async Task<ActionResult<SeatDTO>> Add(SeatDTO seat)
        {
            var result = await _seatService.Add(seat);

            return Ok(result);
        }

        [HttpPut]
        [Authorize]

        public async Task<ActionResult<SeatDTO>> Update(SeatDTO seat)
        {
            var result = await _seatService.Update(seat);

            return Ok(result);
        }





    }
}
