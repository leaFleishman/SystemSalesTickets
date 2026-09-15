using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Data
{
    public class EventSeatRepository
        : Repository<EventSeat>, IEventSeatRepository
    {
        public EventSeatRepository(DataContext context)
            : base(context)
        {
        }

        public async Task<EventSeat?> GetByEventAndSeat(
            int eventId,
            int seatId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(es => es.Event)
                .FirstOrDefaultAsync(
                    es => es.EventId == eventId &&
                          es.SeatId == seatId,
                    cancellationToken);
        }

        public async Task<IEnumerable<EventSeat>> GetAllByEvent(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(es => es.Seat)
                .Where(es => es.EventId == eventId)
                .OrderBy(es => es.SeatId)
                .ToListAsync(cancellationToken);
        }
    }
}