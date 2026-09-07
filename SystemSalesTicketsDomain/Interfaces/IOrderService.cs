using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IOrderService
    {
        Task<OrderLogDTO> AddOrder(OrderDTO order, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrderDTO>> GetAllOrders(CancellationToken cancellationToken = default);
        Task<OrderLogDTO> GetOrderById(int id, CancellationToken cancellationToken = default);

    }
}

