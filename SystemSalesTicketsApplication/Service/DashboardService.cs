using Microsoft.Extensions.Logging;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Repository;

namespace SystemSalesTickets.Service.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        public async Task<AdminDashboardDTO> GetAsync(CancellationToken cancellationToken = default)
        {
            var result = await _dashboardRepository.GetAsync(
                DateTime.UtcNow,
                5,
                cancellationToken);

            _logger.LogInformation(
                "Admin dashboard loaded: {ActiveEvents} active events, {TicketsSold} tickets sold",
                result.ActiveEvents,
                result.TicketsSold);

            return result;
        }
    }
}