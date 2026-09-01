using Microsoft.EntityFrameworkCore;
using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsInfrastructure;

namespace SystemSalesTicketsData
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        private readonly DataContext _dataContext;

        public EventRepository(DataContext context) : base(context)
        {
            _dataContext = context;
        }




        public async Task AddEvent(Event e)
        {
            await _dbSet.AddAsync(e);
            await _dataContext.SaveChangesAsync();
        }


        public async Task<IEnumerable<Event>> GetEvents()
        {
            return await _dbSet.ToListAsync();
        }


    }
}
