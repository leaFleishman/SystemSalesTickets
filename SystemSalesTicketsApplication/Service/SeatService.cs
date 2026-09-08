using AutoMapper;
using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;
using MyApp.Application.Common.Models;

namespace SystemSalesTickets.Service.Service
{
    public class SeatService : ISeatService
    {
        //private static int counter = new Random().Next();
        private readonly ISeatRepository _seatRepository;
        private readonly ILogger<SeatService> _logger;

        private readonly IMapper _mapper;


        public SeatService(ISeatRepository seatRepository, IMapper mapper,ILogger<SeatService> logger)
        {
            _seatRepository = seatRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<SeatDTO>> GetAll(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _seatRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
            return new PagedResponse<SeatDTO>(
                _mapper.Map<IEnumerable<SeatDTO>>(result.Data),
                result.PageNumber,
                result.PageSize,
                result.TotalRecords);
        }

        public async Task<SeatLogDTO> GetById(int id, CancellationToken cancellationToken = default)
        {
            var res= await _seatRepository.GetById(id, cancellationToken);
            return _mapper.Map<SeatLogDTO>(res);
        }

        public async Task<SeatLogDTO> Add(SeatDTO seat, CancellationToken cancellationToken = default)
        {
            var tmp = _mapper.Map<Seat>(seat);
            //tmp.SeatId=counter++;
            var res = await _seatRepository.Add(tmp, cancellationToken);
            return _mapper.Map<SeatLogDTO>(res);
        }

        public async Task<bool> DeleteSeatAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _seatRepository.Delete(id, cancellationToken);
        }

    }
}
