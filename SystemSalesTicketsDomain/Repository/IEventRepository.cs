using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<PagedResponse<Event>> GetUpcomingAsync(DateTime fromUtc, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<Event> GetEventByName(string name, CancellationToken cancellationToken = default);
        Task<bool> DateInUse(DateTime date, int excludeEventId, CancellationToken cancellationToken = default);
    }

    public interface IEventSeatRepository : IRepository<EventSeat>
    {
        Task<EventSeat?> GetByEventAndSeat(int eventId, int seatId, CancellationToken cancellationToken = default);
        Task<IEnumerable<EventSeat>> GetAllByEvent(int eventId, CancellationToken cancellationToken = default);
        void Remove(EventSeat eventSeat);
    }
}
