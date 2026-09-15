using SystemSalesTickets.Core.DTOs;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IEventSeatService
    {
        Task<IEnumerable<EventSeatDTO>> GetSeatsForEvent(int eventId, CancellationToken cancellationToken = default);
        Task<EventSeatResultDTO> AddEventSeat(AddEventSeatDTO dto, CancellationToken cancellationToken = default);
        Task<LinkAllSeatsResultDTO> LinkAllSeatsToEvent(int eventId, CancellationToken cancellationToken = default);
    }
}
