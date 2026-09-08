
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace SystemSalesTickets.Data
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {

        public OrderRepository(DataContext context)
            : base(context)
        {
        }

        public async Task<bool> ExistsForEventAndSeat(
            int eventId,
            int seatId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(
                order => order.EventId == eventId && order.SeatId == seatId,
                cancellationToken);
        }

    }
}
