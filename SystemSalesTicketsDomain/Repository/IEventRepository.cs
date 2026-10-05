using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface IEventRepository : IRepository<Event>
    {
        public Task<Event> GetEventByName(string name, CancellationToken cancellationToken = default);

        /// <summary>True if another event (any id except excludeEventId) already starts at exactly this date.</summary>
        Task<bool> DateInUse(DateTime date, int excludeEventId, CancellationToken cancellationToken = default);
    }


    public interface IEventSeatRepository : IRepository<EventSeat>
    {
        Task<EventSeat?> GetByEventAndSeat(
            int eventId,
            int seatId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<EventSeat>> GetAllByEvent(
            int eventId,
            CancellationToken cancellationToken = default);

        void Remove(EventSeat eventSeat);
    }
}
