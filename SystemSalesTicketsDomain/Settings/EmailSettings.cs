namespace SystemSalesTickets.Core.Settings
{
    /// <summary>
    /// SMTP email configuration.
    /// Bound from the "Email" section of appsettings / environment variables.
    /// Keep the SMTP password out of source control.
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "SystemSalesTickets";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(SmtpHost) &&
            SmtpPort > 0 &&
            SmtpPort <= 65535 &&
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(FromAddress);
    }
}