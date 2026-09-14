using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Data
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        private readonly DataContext _dataContext;

        public SeatRepository(DataContext dataContext)
            : base(dataContext)
        {
        }
        public async Task<List<Seat>> GetAllSeats(CancellationToken cancellationToken = default)

        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

    }
}
