using SystemSalesTickets.Core.DTOs;
using MyApp.Application.Common.Models;


namespace SystemSalesTickets.Core.Interfaces
{
    public interface ISeatService
    {
        Task<PagedResponse<SeatDTO>> GetAll(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<SeatLogDTO> GetById(int id, CancellationToken cancellationToken = default);
        Task<SeatLogDTO> Add(SeatDTO seat, CancellationToken cancellationToken = default);
        Task<bool> DeleteSeatAsync(int id, CancellationToken cancellationToken = default);
    }
}
