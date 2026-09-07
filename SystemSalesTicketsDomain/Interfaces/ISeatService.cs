using SystemSalesTickets.Core.DTOs;


namespace SystemSalesTickets.Core.Interfaces
{
    public interface ISeatService
    {
        Task<IEnumerable<SeatDTO>> GetAll(CancellationToken cancellationToken = default);
        Task<SeatLogDTO> GetById(int id, CancellationToken cancellationToken = default);
        Task<SeatLogDTO> Add(SeatDTO seat, CancellationToken cancellationToken = default);
        Task<bool> DeleteSeatAsync(int id, CancellationToken cancellationToken = default);
    }
}
