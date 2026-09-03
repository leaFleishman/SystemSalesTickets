using Microsoft.AspNetCore.Mvc;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;

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
        public async Task<ActionResult<IEnumerable<SeatDTO>>> GetAll()
        {
            return Ok(await _seatService.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SeatDTO>> GetById(int id)
        {
            var seat = await _seatService.GetById(id);

            if (seat == null)
                return NotFound();

            return Ok(seat);
        }

        [HttpPost]
        public async Task<ActionResult<SeatDTO>> Add(SeatDTO seat)
        {
            var result = await _seatService.Add(seat);

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<SeatDTO>> Update(SeatDTO seat)
        {
            var result = await _seatService.Update(seat);

            return Ok(result);
        }





    }
}
