using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<List<Seat>> GetAllSeats(
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            int row,
            int line,
            CancellationToken cancellationToken = default);

        Task<PagedResponse<Seat>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 20,
            int? row = null,
            CancellationToken cancellationToken = default);
    }
}