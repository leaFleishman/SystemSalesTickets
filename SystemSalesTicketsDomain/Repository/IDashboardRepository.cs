using SystemSalesTickets.Core.DTOs;

namespace SystemSalesTickets.Core.Repository
{
    public interface IDashboardRepository
    {
        Task<AdminDashboardDTO> GetAsync(
            DateTime fromUtc,
            int upcomingLimit = 5,
            CancellationToken cancellationToken = default);
    }
}