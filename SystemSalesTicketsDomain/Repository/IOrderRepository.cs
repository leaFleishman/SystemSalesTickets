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

    }
}
