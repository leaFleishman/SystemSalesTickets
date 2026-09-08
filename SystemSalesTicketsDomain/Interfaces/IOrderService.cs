using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResultDTO> AddOrder(OrderDTO order, CancellationToken cancellationToken = default);
        Task<PagedResponse<OrderDTO>> GetAllOrders(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<OrderLogDTO> GetOrderById(int id, CancellationToken cancellationToken = default);

    }
}

