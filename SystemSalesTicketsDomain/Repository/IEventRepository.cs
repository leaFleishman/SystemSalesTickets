
using SystemSalesTickets.Core.Repository;
using SystemSalesTicketsDomain.models;

namespace SystemSalesTicketsCore.Repository
{
    public interface IEventRepository:IRepository<Event>
    {
        public  Task<IEnumerable<Event>> GetEvents();

        Task AddEvent(Event e);




    }
}
