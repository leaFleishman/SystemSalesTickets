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
    }
}
