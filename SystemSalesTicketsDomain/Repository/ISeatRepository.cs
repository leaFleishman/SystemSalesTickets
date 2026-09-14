
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Repository
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<List<Seat>> GetAllSeats(CancellationToken cancellationToken = default);
    }
}
