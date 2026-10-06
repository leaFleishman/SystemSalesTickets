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
            _dataContext = dataContext;
        }

        public async Task<List<Seat>> GetAllSeats(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResponse<Seat>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 20,
            int? row = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dataContext.Seats.AsNoTracking();

            if (row.HasValue)
            {
                query = query.Where(s => s.Row == row.Value);
            }

            var count = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<Seat>(
                items,
                pageNumber,
                pageSize,
                count);
        }
    }
}