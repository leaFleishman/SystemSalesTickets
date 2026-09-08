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
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetAll seats request received");

            var seats = await _seatService.GetAll(pageNumber, pageSize, cancellationToken);

            _logger.LogInformation("GetAll returned page {PageNumber} with {Count} seats", seats.PageNumber, seats.Data.Count());

            return Ok(seats);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<SeatDTO>> GetById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetById request received for SeatId {SeatId}", id);

            var seat = await _seatService.GetById(id, cancellationToken);

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
        public async Task<ActionResult<SeatDTO>> Add([FromBody] SeatDTO seat, CancellationToken cancellationToken)
        {
            if (seat == null)
            {
                return BadRequest("Invalid seat data.");
            }

            _logger.LogInformation("Add seat request received");

            var result = await _seatService.Add(seat, cancellationToken);

            if (result == null)
            {
                return BadRequest("Failed to create seat.");
            }

            _logger.LogInformation("Add seat completed successfully for SeatId {SeatId}", result.Id);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> DeleteAsyncSeat(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Delete seat request received for SeatId {SeatId}", id);

            var deleted = await _seatService.DeleteSeatAsync(id, cancellationToken);

            if (!deleted)
            {
                _logger.LogWarning("Delete: no seat found for SeatId {SeatId}", id);
                return NotFound();
            }

            _logger.LogInformation("Delete succeeded for SeatId {SeatId}", id);
            return NoContent();
        }
    }
}