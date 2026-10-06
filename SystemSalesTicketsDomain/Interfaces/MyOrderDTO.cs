namespace SystemSalesTickets.Core.DTOs
{
    /// <summary>A customer's booking, flattened for the "My bookings" screen.</summary>
    public class MyOrderDTO
    {
        public int OrderId { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        /// <summary>Event start (UTC).</summary>
        public DateTime EventDate { get; set; }

        public decimal Price { get; set; }

        public int SeatId { get; set; }

        public int Row { get; set; }

        public int Line { get; set; }

        /// <summary>UTC time the booking was made.</summary>
        public DateTime OrderDate { get; set; }

        public bool EventIsCancelled { get; set; }

        /// <summary>True if the customer may still cancel (event starts in more than 24h and is not cancelled).</summary>
        public bool CanCancel { get; set; }

        /// <summary>UTC deadline for cancelling (event start minus 24h).</summary>
        public DateTime CancellationDeadline { get; set; }
    }
}
