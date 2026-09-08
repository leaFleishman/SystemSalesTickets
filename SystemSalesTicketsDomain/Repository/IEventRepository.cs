using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface IEventRepository : IRepository<Event>
    {
        public Task<Event> GetEventByName(string name, CancellationToken cancellationToken = default);
    }


    public interface IEventSeatRepository : IRepository<EventSeat>
    {
        Task<EventSeat?> GetByEventAndSeat(
            int eventId,
            int seatId,
            CancellationToken cancellationToken = default);
    }
}
