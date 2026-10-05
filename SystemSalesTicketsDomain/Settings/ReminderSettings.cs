namespace SystemSalesTickets.Core.Settings
{
    /// <summary>
    /// Configuration of the event reminder job. Bound from the "Reminder" section.
    /// </summary>
    public class ReminderSettings
    {
        public const string SectionName = "Reminder";

        /// <summary>Turns the whole background job on/off.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>How long before the event the reminder is sent.</summary>
        public int HoursBeforeEvent { get; set; } = 24;

        /// <summary>How often the job looks for reminders that are due.</summary>
        public int CheckIntervalMinutes { get; set; } = 15;

        /// <summary>
        /// Time zone used to display the event time in the email (IANA or Windows id).
        /// Falls back to UTC if the id is not found on the server.
        /// </summary>
        public string TimeZoneId { get; set; } = "Asia/Jerusalem";
    }
}
