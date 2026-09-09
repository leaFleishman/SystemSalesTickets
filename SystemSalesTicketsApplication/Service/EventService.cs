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
        private readonly ISeatRepository _seatRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventSeatRepository _eventSeatRepository;
        private readonly IMapper _mapper;

        public EventService(IEventRepository eventRepo, IMapper mapper, ILogger<EventService> logger,ISeatRepository seatRepository, IEventSeatRepository eventSeatRepository)
        {
            _eventRepository = eventRepo;
            _mapper = mapper;
            _logger = logger;
            _seatRepository = seatRepository;
            _eventSeatRepository = eventSeatRepository;
        }



        public async Task<EventDTO> Add(
      EventDTO e,
      CancellationToken cancellationToken = default)
        {
            var tmp = _mapper.Map<Event>(e);

            await _eventRepository.Add(tmp, cancellationToken);

            var seats = await _seatRepository.GetAllAsync(
                1,
                int.MaxValue,
                cancellationToken);

            foreach (var seat in seats.Data)
            {
                await _eventSeatRepository.Add(
                    new EventSeat
                    {
                        Event = tmp,
                        SeatId = seat.Id,
                        IsAvailable = true
                    },
                    cancellationToken);
            }

            await _eventRepository.Save(cancellationToken);

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
