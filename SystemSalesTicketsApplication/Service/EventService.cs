using AutoMapper;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace SystemSalesTickets.Service.Service
{
    public class EventService : IEventService
    {

        private readonly ILogger<EventService> _logger;
        private static int counter = new Random().Next();
        private readonly IEventRepository _eventRepository;

        private readonly IMapper _mapper;

        public EventService(IEventRepository eventRepo, IMapper mapper, ILogger<EventService> logger)
        {
            _eventRepository = eventRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Add(EventDTO e)
        {
            var tmp = _mapper.Map<Event>(e);
            tmp.EventId = counter++;
            await _eventRepository.Add(tmp);
        }

        public async Task<EventDTO> AddEvent(EventDTO e)
        {
            var tmp = _mapper.Map<Event>(e);
            tmp.EventId = counter++;
            tmp.Version++;
            _eventRepository.Add(tmp);

            return _mapper.Map<EventDTO>(tmp);
        }



        public async Task<IEnumerable<EventDTO>> GetAll()
        {
            var tmp = await _eventRepository.GetAll();
            return _mapper.Map<IEnumerable<EventDTO>>(tmp);
        }

        public async Task<EventDTO> GetEventByName(string name)
        {
          var tmp=await  _eventRepository.GetEventByName(name);
            return _mapper.Map<EventDTO>(tmp);
        }
    }
}
