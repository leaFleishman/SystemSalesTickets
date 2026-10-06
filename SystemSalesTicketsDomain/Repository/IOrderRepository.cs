using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<bool> ExistsForEventAndSeat(int eventId, int seatId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Orders not yet reminded whose event starts in (fromUtc, toUtc]. Cancelled events are skipped.
        /// Includes User, Event and Seat (tracked).
        /// </summary>
        Task<List<Order>> GetOrdersPendingReminder(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

        /// <summary>All orders of an event, with User and Seat loaded (not tracked).</summary>
        Task<List<Order>> GetOrdersByEvent(int eventId, CancellationToken cancellationToken = default);

        Task<int> CountByEvent(int eventId, CancellationToken cancellationToken = default);

        /// <summary>All bookings of a customer (newest first), with Event and Seat loaded (not tracked).</summary>
        Task<List<Order>> GetOrdersByUser(int userId, CancellationToken cancellationToken = default);

        /// <summary>One order with Event loaded, tracked so it can be removed in the same unit of work.</summary>
        Task<Order?> GetByIdForCancellation(int id, CancellationToken cancellationToken = default);

        /// <summary>Marks the order for deletion. Call Save() to commit.</summary>
        void Remove(Order order);

    }
}
