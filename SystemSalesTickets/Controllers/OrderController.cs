using Microsoft.AspNetCore.Mvc;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("AddOrder")]
        public async Task<ActionResult<Order>> AddOrder([FromBody] Order order)
        {
            var result = await _orderService.AddOrder(order);
            return Ok(result);
        }

        [HttpGet("GetAllOrders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrders();
            return Ok(orders);
        }

        [HttpGet("GetOrderById")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderById(id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }

}

