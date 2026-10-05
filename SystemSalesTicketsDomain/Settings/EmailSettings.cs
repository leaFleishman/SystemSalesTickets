namespace SystemSalesTickets.Core.Settings
{
    /// <summary>
    /// SMTP configuration. Bound from the "Email" section of appsettings / environment variables.
    /// Keep the password out of source control: set it with the environment variable Email__Password.
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FromAddress { get; set; } = string.Empty;

        public string FromName { get; set; } = "SystemSalesTickets";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Host) &&
            !string.IsNullOrWhiteSpace(FromAddress);
    }
}
