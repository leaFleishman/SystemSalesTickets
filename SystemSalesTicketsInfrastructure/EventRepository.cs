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

        public async Task<Event> GetEventByName(string name)
        {
            return await _dataContext.Events
                   .FirstOrDefaultAsync(e => e.Name == name);
        }
    }
}
