using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;

namespace SystemSalesTickets.Service.Service
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepo)
        {
            _eventRepository = eventRepo;
        }

        public async Task AddEvent(Event e)
        {
            await _eventRepository.AddEvent(e);
        }

        public async Task<IEnumerable<Event>> GetEvents()
        {
            return await _eventRepository.GetEvents();
        }
    }
}
