namespace SystemSalesTickets.Core.Enums
{
    public enum EventResultStatus
    {
        Success,
        NotFound,
        /// <summary>The request is valid but clashes with the current state (cancelled, past, duplicate date...).</summary>
        Conflict,
        /// <summary>The request itself is invalid (bad name, date in the past...).</summary>
        Invalid
    }
}
