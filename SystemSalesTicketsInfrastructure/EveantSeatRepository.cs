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
            return await _dbSet.FirstOrDefaultAsync(
                es => es.EventId == eventId &&
                      es.SeatId == seatId,
                cancellationToken);
        }
    }
}