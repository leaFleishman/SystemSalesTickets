
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

        public async Task<List<Order>> GetOrdersPendingReminder(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default)
        {
            // Tracked on purpose: the reminder job sets ReminderSentAt and calls Save().
            // Cancelled events never get a reminder.
            return await _dbSet
                .Include(o => o.User)
                .Include(o => o.Event)
                .Include(o => o.Seat)
                .Where(o => o.ReminderSentAt == null
                            && !o.Event.IsCancelled
                            && o.Event.Date > fromUtc
                            && o.Event.Date <= toUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Order>> GetOrdersByEvent(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Seat)
                .Where(o => o.EventId == eventId)
                .OrderBy(o => o.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountByEvent(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(o => o.EventId == eventId, cancellationToken);
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
