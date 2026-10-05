using SystemSalesTickets.Core.Enums;

namespace SystemSalesTickets.Core.DTOs
{
    public class EventResultDTO
    {
        public EventResultStatus Status { get; set; }

        public EventDTO? Event { get; set; }

        public string? Message { get; set; }

        /// <summary>Cancel only: number of orders (tickets) that exist for the cancelled event.</summary>
        public int AffectedOrders { get; set; }

        /// <summary>Cancel only: number of customers that were sent a cancellation email.</summary>
        public int NotificationsSent { get; set; }
    }
}
