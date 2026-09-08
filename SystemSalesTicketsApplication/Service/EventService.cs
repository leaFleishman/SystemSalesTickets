using AutoMapper;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.DTOs;
using Microsoft.Extensions.Logging;
using MyApp.Application.Common.Models;

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


        public async Task<PagedResponse<EventDTO>> GetAll(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _eventRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
            return new PagedResponse<EventDTO>(
                _mapper.Map<IEnumerable<EventDTO>>(result.Data),
                result.PageNumber,
                result.PageSize,
                result.TotalRecords);
        }

        public async Task<EventDTO> GetEventByName(string name, CancellationToken cancellationToken = default)
        {
            var tmp = await _eventRepository.GetEventByName(name, cancellationToken);
            return _mapper.Map<EventDTO>(tmp);
        }

       
    }
}
