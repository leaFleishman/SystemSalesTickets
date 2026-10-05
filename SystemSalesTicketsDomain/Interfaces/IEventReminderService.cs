namespace SystemSalesTickets.Core.Interfaces
{
    public interface IEventReminderService
    {
        /// <summary>
        /// Sends a reminder to every customer who has a ticket for an event that starts
        /// within the configured window and has not been reminded yet.
        /// </summary>
        /// <returns>The number of emails that were sent.</returns>
        Task<int> SendDueRemindersAsync(CancellationToken cancellationToken = default);
    }
}
