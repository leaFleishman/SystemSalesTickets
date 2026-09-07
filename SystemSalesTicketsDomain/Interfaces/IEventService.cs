using SystemSalesTickets.Core.DTOs;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IEventService
    {
        public Task<IEnumerable<EventDTO>> GetAll();

        Task<EventDTO> Add(EventDTO e);
        public Task<EventDTO> GetEventByName(string name);
    }
}
