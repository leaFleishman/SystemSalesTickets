namespace SystemSalesTickets.Core.Settings
{
    /// <summary>
    /// HTTPS email API configuration.
    /// Bound from the "Email" section of appsettings / environment variables.
    /// Keep the API key out of source control.
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string ApiKey { get; set; } = string.Empty;

        public string FromAddress { get; set; } = string.Empty;

        public string FromName { get; set; } = "SystemSalesTickets";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ApiKey) &&
            !string.IsNullOrWhiteSpace(FromAddress);
    }
}