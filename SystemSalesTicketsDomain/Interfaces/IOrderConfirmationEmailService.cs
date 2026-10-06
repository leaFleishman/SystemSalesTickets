using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IOrderConfirmationEmailService
    {
        Task SendAsync(
            Order order,
            string email,
            string? userName,
            CancellationToken cancellationToken = default);

        Task SendCancellationAsync(
            Order order,
            string email,
            string? userName,
            CancellationToken cancellationToken = default);
    }
}