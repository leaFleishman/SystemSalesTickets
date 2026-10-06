using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Data
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        private readonly DataContext _dataContext;

        public EventRepository(DataContext context) : base(context)
        {
            _dataContext = context;
        }

        public async Task<PagedResponse<Event>> GetUpcomingAsync(
            DateTime fromUtc,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _dataContext.Events
                .AsNoTracking()
                .Where(e => e.Date > fromUtc)
                .OrderBy(e => e.Date);

            var count = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<Event>(items, pageNumber, pageSize, count);
        }

        public async Task<Event> GetEventByName(string name, CancellationToken cancellationToken = default)
        {
            return await _dataContext.Events.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
        }

        public async Task<bool> DateInUse(DateTime date, int excludeEventId, CancellationToken cancellationToken = default)
        {
            return await _dataContext.Events.AsNoTracking()
                .AnyAsync(e => e.Date == date && e.Id != excludeEventId, cancellationToken);
        }
    }
}
