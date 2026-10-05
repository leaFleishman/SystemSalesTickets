using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Settings;

namespace SystemSalesTickets.Service.Service
{
    public sealed class GmailEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<GmailEmailService> _logger;

        public GmailEmailService(
            IOptions<EmailSettings> settings,
            ILogger<GmailEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(
            string toAddress,
            string? toName,
            string subject,
            string htmlBody,
            string? textBody,
            CancellationToken cancellationToken = default)
        {
            if (!_settings.IsConfigured)
            {
                throw new InvalidOperationException(
                    "Email is not configured. Set Email:SmtpHost, Email:SmtpPort, Email:Username, Email:Password and Email:FromAddress.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(new MailboxAddress(toName ?? string.Empty, toAddress));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = textBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await smtpClient.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                cancellationToken);

            await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Email '{Subject}' sent via Gmail SMTP to {Address}",
                subject,
                toAddress);
        }
    }
}