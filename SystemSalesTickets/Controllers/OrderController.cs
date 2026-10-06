using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderLogDTO>> AddOrder(
            [FromBody] OrderDTO order,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("AddOrder request received");

            var result = await _orderService.AddOrder(
                order,
                cancellationToken);

            switch (result.Status)
            {
                case OrderResultStatus.NotFound:

                    _logger.LogWarning(
                        "AddOrder failed: {Message}",
                        result.Message);

                    return NotFound(result.Message);

                case OrderResultStatus.Conflict:

                    _logger.LogWarning(
                        "AddOrder conflict: {Message}",
                        result.Message);

                    return Conflict(result.Message);

                case OrderResultStatus.Success:

                    _logger.LogInformation(
                        "AddOrder completed successfully for OrderId {OrderId}",
                        result.Order!.Id);

                    return CreatedAtAction(
                        nameof(GetOrderById),
                        new { id = result.Order.Id },
                        result.Order);

                default:
                    return BadRequest();
            }
        }
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Manager))]

        public async Task<IActionResult> GetAllOrders([FromQuery]int pageNumber = 1,[FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetAllOrders request received by user");

            var orders = await _orderService.GetAllOrders(pageNumber, pageSize, cancellationToken);

            _logger.LogInformation("GetAllOrders returned page {PageNumber} with {Count} orders", orders.PageNumber, orders.Data.Count());

            return Ok(orders);
        }

        [Authorize(Roles = nameof(UserRole.Manager))]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderLogDTO>> GetOrderById([FromRoute]
      int id,
      CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "GetOrderById request received for OrderId {OrderId}",
                id);

            var order = await _orderService.GetOrderById(
                id,
                cancellationToken);

            if (order == null)
            {
                _logger.LogWarning(
                    "GetOrderById: no order found for OrderId {OrderId}",
                    id);

                return NotFound();
            }

            _logger.LogInformation(
                "GetOrderById succeeded for OrderId {OrderId}",
                id);

            return Ok(order);
        }

        /// <summary>The logged-in customer's bookings (event name, date/time, price, seat).</summary>
        [Authorize]
        [HttpGet("my")]
        public async Task<ActionResult<IReadOnlyList<MyOrderDTO>>> GetMyOrders(CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var orders = await _orderService.GetMyOrders(userId, cancellationToken);

            _logger.LogInformation("GetMyOrders returned {Count} orders for user {UserId}", orders.Count, userId);

            return Ok(orders);
        }

        /// <summary>Cancels one of the caller's own bookings; allowed only more than 24h before the event.</summary>
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int id, CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var result = await _orderService.CancelOrder(id, userId, cancellationToken);

            switch (result.Status)
            {
                case OrderResultStatus.NotFound:
                    _logger.LogWarning("CancelOrder failed for OrderId {OrderId}: {Message}", id, result.Message);
                    return NotFound(result.Message);

                case OrderResultStatus.Conflict:
                    _logger.LogWarning("CancelOrder rejected for OrderId {OrderId}: {Message}", id, result.Message);
                    return Conflict(result.Message);

                case OrderResultStatus.Success:
                    return NoContent();

                default:
                    return BadRequest();
            }
        }

        // The user id always comes from the signed token, never from the request.
        private bool TryGetCurrentUserId(out int userId)
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out userId);
        }
    }
}