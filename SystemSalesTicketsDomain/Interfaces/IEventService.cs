using SystemSalesTickets.Core.DTOs;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IEventService
    {
        public Task<IEnumerable<EventDTO>> GetAll(CancellationToken cancellationToken = default);

        Task<EventDTO> Add(EventDTO e, CancellationToken cancellationToken = default);
        public Task<EventDTO> GetEventByName(string name, CancellationToken cancellationToken = default);
    }
}
