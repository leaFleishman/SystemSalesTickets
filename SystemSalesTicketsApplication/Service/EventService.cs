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

       

        public async Task<EventDTO> Add(EventDTO e, CancellationToken cancellationToken = default)
        {
            var tmp = _mapper.Map<Event>(e);
            tmp.EventId = counter++;
            await _eventRepository.Add(tmp, cancellationToken);
            return _mapper.Map<EventDTO>(tmp);
        }


        public async Task<IEnumerable<EventDTO>> GetAll(CancellationToken cancellationToken = default)
        {
            var tmp = await _eventRepository.GetAll(cancellationToken);
            return _mapper.Map<IEnumerable<EventDTO>>(tmp);
        }

        public async Task<EventDTO> GetEventByName(string name, CancellationToken cancellationToken = default)
        {
            var tmp = await _eventRepository.GetEventByName(name, cancellationToken);
            return _mapper.Map<EventDTO>(tmp);
        }

       
    }
}
