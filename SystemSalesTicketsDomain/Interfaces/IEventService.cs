using SystemSalesTickets.Core.DTOs;
using MyApp.Application.Common.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IEventService
    {
        public Task<PagedResponse<EventDTO>> GetAll(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

        Task<EventDTO> Add(EventDTO e, CancellationToken cancellationToken = default);
        public Task<EventDTO> GetEventByName(string name, CancellationToken cancellationToken = default);
    }
}
