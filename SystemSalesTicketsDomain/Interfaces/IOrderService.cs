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

        /// <summary>All bookings of the given customer, newest event first.</summary>
        Task<IReadOnlyList<MyOrderDTO>> GetMyOrders(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancels a booking owned by <paramref name="userId"/> and releases its seat.
        /// Allowed only if the event starts in MORE than 24 hours.
        /// </summary>
        Task<OrderResultDTO> CancelOrder(int orderId, int userId, CancellationToken cancellationToken = default);

    }
}

