using AutoMapper;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.DTOs;

namespace SystemSalesTickets.Service.Service
{
    public class EventService : IEventService
    {
        private static int counter = 1;

        private readonly IEventRepository _eventRepository;

        private readonly IMapper _mapper;

        public EventService(IEventRepository eventRepo, IMapper mapper)
        {
            _eventRepository = eventRepo;
            _mapper = mapper;
        }

        public async Task Add(EventDTO e)
        {
            var tmp = _mapper.Map<Event>(e);
            tmp.EventId = counter++;
            await _eventRepository.Add(tmp);
        }

        public async Task<IEnumerable<EventDTO>> GetAll()
        {
            var tmp = await _eventRepository.GetAll();
            return _mapper.Map<IEnumerable<EventDTO>>(tmp);
        }
    }
}
