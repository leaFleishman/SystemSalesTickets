using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Data
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DataContext _dataContext;

        public DashboardRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<AdminDashboardDTO> GetAsync(
            DateTime fromUtc,
            int upcomingLimit = 5,
            CancellationToken cancellationToken = default)
        {
            var activeEvents = await _dataContext.Events
                .AsNoTracking()
                .Where(e => !e.IsCancelled && e.Date > fromUtc)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Date,
                    e.Price,
                    e.NumberOfSeats,
                    TicketsSold = _dataContext.Orders.Count(o => o.EventId == e.Id)
                })
                .OrderBy(e => e.Date)
                .ToListAsync(cancellationToken);

            var ticketsSold = activeEvents.Sum(e => e.TicketsSold);
            var revenue = activeEvents.Sum(e => (decimal)e.TicketsSold * (decimal)e.Price);
            var availableSeats = activeEvents.Sum(e => Math.Max(0, e.NumberOfSeats - e.TicketsSold));

            var upcoming = activeEvents
                .Take(upcomingLimit)
                .Select(e => new DashboardEventDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    TicketsSold = e.TicketsSold,
                    AvailableSeats = Math.Max(0, e.NumberOfSeats - e.TicketsSold)
                })
                .ToList();

            return new AdminDashboardDTO
            {
                ActiveEvents = activeEvents.Count,
                TicketsSold = ticketsSold,
                Revenue = revenue,
                AvailableSeats = availableSeats,
                UpcomingEvents = upcoming
            };
        }
    }
}