using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;
using AutoMapper;

namespace SystemSalesTickets.Service.Service
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        
        private readonly IMapper _mapper;

        public EventService(IEventRepository eventRepo,IMapper mapper)
        {
            _eventRepository = eventRepo;
            _mapper = mapper;
        }

        public async Task Add(Event e)
        {
            await _eventRepository.Add(e);
        }

        public async Task<IEnumerable<Event>> GetAll()
        {
            return await _eventRepository.GetAll();
        }
    }
}
