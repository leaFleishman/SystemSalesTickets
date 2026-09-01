using SystemSalesTicketsDomain.models;

namespace SystemSalesTicketsPresentation.Interfaces
{
    public interface IEventService
    {
        public Task<IEnumerable<Event>> GetEvents();

        public Task AddEvent(Event e);
    }
}
