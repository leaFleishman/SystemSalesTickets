using SystemSalesTickets.Core.DTOs;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardDTO> GetAsync(
            CancellationToken cancellationToken = default);
    }
}