using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<bool> ExistsForEventAndSeat(int eventId, int seatId, CancellationToken cancellationToken = default);

    }
}
