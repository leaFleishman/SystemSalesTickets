using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<OrderLogDTO>> AddOrder([FromBody] OrderDTO order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("AddOrder request received");

            var result = await _orderService.AddOrder(
                order,
                cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("AddOrder failed");
                return Conflict("Seat was just booked by someone else, please try again");
            }

            if (!string.IsNullOrEmpty(result.Message))
            {
                _logger.LogWarning(
                    "AddOrder failed: {Message}",
                    result.Message);

                if (result.Message == "Seat not found")
                    return NotFound(result.Message);

                if (result.Message == "Seat is already occupied" ||
                    result.Message == "Seat was just booked by someone else, please try again")
                {
                    return Conflict(result.Message);
                }
            }

            _logger.LogInformation(
                "AddOrder completed successfully for OrderId {OrderId}",
                result.Id);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = result.Id },
                result);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Manager))]

        public async Task<IActionResult> GetAllOrders(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
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
    }
}