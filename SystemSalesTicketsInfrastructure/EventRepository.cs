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

        public async Task<Event> GetEventByName(string name, CancellationToken cancellationToken = default)
        {
            return await _dataContext.Events.AsNoTracking()

                   .FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
        }

        public async Task<bool> DateInUse(DateTime date, int excludeEventId, CancellationToken cancellationToken = default)
        {
            // Events.Date has a unique index, so two events can never share a start time.
            return await _dataContext.Events.AsNoTracking()
                .AnyAsync(e => e.Date == date && e.Id != excludeEventId, cancellationToken);
        }
    }
}
