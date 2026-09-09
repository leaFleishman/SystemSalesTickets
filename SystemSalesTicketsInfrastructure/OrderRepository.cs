
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
            int Id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(
                order => order.EventId == eventId && order.SeatId == Id,
                cancellationToken);
        }

        public async Task<Order?> GetById(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(o => o.Event)
                .Include(o => o.Seat)
                .FirstOrDefaultAsync(
                    o => o.Id == id,
                    cancellationToken);
        }

    }
}
