using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core.Interfaces
{
    public interface IEventCancellationNotifier
    {
        /// <summary>
        /// Sends one cancellation email per customer (even if they hold several tickets).
        /// Orders must have User loaded. Returns the number of emails sent; a failure for one
        /// customer never stops the others.
        /// </summary>
        Task<int> NotifyAsync(
            Event cancelledEvent,
            IReadOnlyCollection<Order> orders,
            string? reason,
            CancellationToken cancellationToken = default);
    }
}
