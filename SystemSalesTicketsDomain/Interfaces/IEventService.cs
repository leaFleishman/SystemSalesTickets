using SystemSalesTicketsDomain.models;

namespace SystemSalesTicketsPresentation.Interfaces
{
    public interface IEventService
    {
        public Task<IEnumerable<Event>> GetAll();

        public Task Add(Event e);
    }
}
