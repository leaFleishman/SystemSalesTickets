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




        


    }
}
