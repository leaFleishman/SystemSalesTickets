using SystemSalesTicketsCore.Repository;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;

namespace SystemSalesTickets.Service.Service
{
    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _seatRepository;

        public SeatService(ISeatRepository seatRepository)
        {
            _seatRepository = seatRepository;
        }

        public async Task<IEnumerable<Seat>> GetAll()
        {
            return await _seatRepository.GetAll();
        }

        public async Task<Seat?> GetById(int id)
        {
            return await _seatRepository.GetById(id);
        }

        public async Task<Seat> Add(Seat seat)
        {
            return await _seatRepository.Add(seat);
        }

        public async Task<Seat> Update(Seat seat)
        {
            return await _seatRepository.Update(seat);
        }


    }
}
