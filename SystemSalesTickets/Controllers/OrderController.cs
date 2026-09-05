using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;

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
        [HttpPost("AddOrder")]
        public async Task<ActionResult<OrderDTO>> AddOrder([FromBody] OrderDTO order)
        {
            _logger.LogInformation("AddOrder request received");

            try
            {
                var result = await _orderService.AddOrder(order);
                _logger.LogInformation("AddOrder completed successfully for OrderId {OrderId}", result?.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddOrder failed");
                throw;
            }
        }

        [HttpGet("GetAllOrders")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllOrders()
        {
            _logger.LogInformation("GetAllOrders request received by user");

            var orders = await _orderService.GetAllOrders();

            _logger.LogInformation("GetAllOrders returned {Count} orders", orders?.Count() ?? 0);

            return Ok(orders);
        }

        [Authorize]
        [HttpGet("GetOrderById")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            _logger.LogInformation("GetOrderById request received for OrderId {OrderId}", id);

            var order = await _orderService.GetOrderById(id);

            if (order == null)
            {
                _logger.LogWarning("GetOrderById: no order found for OrderId {OrderId}", id);
                return NotFound();
            }

            _logger.LogInformation("GetOrderById succeeded for OrderId {OrderId}", id);
            return Ok(order);
        }
    }
}