using AutoMapper;

using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Service.Service
{
    public class SeatService : ISeatService
    {
        private static int counter = new Random().Next();
        private readonly ISeatRepository _seatRepository;

        private readonly IMapper _mapper;

        public SeatService(ISeatRepository seatRepository, IMapper mapper)
        {
            _seatRepository = seatRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SeatDTO>> GetAll()
        {
            var tmp = await _seatRepository.GetAll();
            return _mapper.Map<IEnumerable<SeatDTO>>(tmp);
        }

        public async Task<SeatDTO> GetById(int id)
        {
            var res= await _seatRepository.GetById(id);
            return _mapper.Map<SeatDTO>(res);
        }

        public async Task<SeatDTO> Add(SeatDTO seat)
        {
            var tmp = _mapper.Map<Seat>(seat);
            tmp.SeatId=counter++;
            var res = await _seatRepository.Add(tmp);
            return _mapper.Map<SeatDTO>(res);
        }

        public async Task<SeatDTO> Update(SeatDTO seat)
        {
            var tmp= _mapper.Map<Seat>(seat);
            await _seatRepository.Update(tmp);
            return _mapper.Map<SeatDTO>(tmp);
        }


    }
}
