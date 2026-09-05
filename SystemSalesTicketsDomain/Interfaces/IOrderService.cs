using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IOrderService
    {
        Task<OrderLogDTO> AddOrder(OrderDTO order);
        Task<IEnumerable<OrderDTO>> GetAllOrders();
        Task<OrderLogDTO> GetOrderById(int id);

    }
}

