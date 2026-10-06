using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface ISeatService
    {
        Task<PagedResponse<SeatDTO>> GetAll(
            int pageNumber = 1,
            int pageSize = 20,
            int? row = null,
            CancellationToken cancellationToken = default);

        Task<SeatLogDTO> GetById(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            int row,
            int line,
            CancellationToken cancellationToken = default);

        Task<SeatLogDTO> Add(
            SeatDTO seat,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteSeatAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}