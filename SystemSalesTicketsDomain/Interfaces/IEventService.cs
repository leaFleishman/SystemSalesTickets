using SystemSalesTickets.Core.DTOs;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IEventService
    {
        public Task<IEnumerable<EventDTO>> GetAll();

        public Task Add(EventDTO e);
    }
}
