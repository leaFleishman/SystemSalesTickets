using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IEventService
    {
        public Task<PagedResponse<EventDTO>> GetAll(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

        Task<EventDTO> Add(EventDTO e, CancellationToken cancellationToken = default);
        public Task<EventDTO> GetEventByName(string name, CancellationToken cancellationToken = default);

        /// <summary>Edits name / date / price / number of seats of an active, upcoming event.</summary>
        Task<EventResultDTO> Update(int id, UpdateEventDTO dto, CancellationToken cancellationToken = default);

        /// <summary>Cancels an active, upcoming event and notifies the customers that hold tickets.</summary>
        Task<EventResultDTO> Cancel(int id, string? reason, CancellationToken cancellationToken = default);
    }
}
