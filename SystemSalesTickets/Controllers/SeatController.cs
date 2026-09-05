using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly ILogger<SeatController> _logger;

        public SeatController(ISeatService seatService, ILogger<SeatController> logger)
        {
            _seatService = seatService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SeatDTO>>> GetAll()
        {
            _logger.LogInformation("GetAll seats request received");

            var seats = await _seatService.GetAll();

            _logger.LogInformation("GetAll returned {Count} seats", seats?.Count() ?? 0);

            return Ok(seats);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<SeatDTO>> GetById(int id)
        {
            _logger.LogInformation("GetById request received for SeatId {SeatId}", id);

            var seat = await _seatService.GetById(id);

            if (seat == null)
            {
                _logger.LogWarning("GetById: no seat found for SeatId {SeatId}", id);
                return NotFound();
            }

            _logger.LogInformation("GetById succeeded for SeatId {SeatId}", id);
            return Ok(seat);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SeatDTO>> Add(SeatDTO seat)
        {
            _logger.LogInformation("Add seat request received");

            try
            {
                var result = await _seatService.Add(seat);
                _logger.LogInformation("Add seat completed successfully for SeatId {SeatId}", result?.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add seat failed");
                throw;
            }
        }

    }
}