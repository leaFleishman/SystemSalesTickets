namespace SystemSalesTickets.Core.Settings
{
    /// <summary>
    /// Google Apps Script email gateway configuration.
    /// Bound from the "Email" section of appsettings / environment variables.
    /// Keep the shared secret out of source control.
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string ApiUrl { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string FromName { get; set; } = "SystemSalesTickets";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ApiUrl) &&
            !string.IsNullOrWhiteSpace(Secret);
    }
}