using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Settings;

namespace SystemSalesTickets.Service.Service
{
    public sealed class ResendEmailService : IEmailService
    {
        private const string Endpoint = "https://api.resend.com/emails";

        private readonly HttpClient _httpClient;
        private readonly EmailSettings _settings;
        private readonly ILogger<ResendEmailService> _logger;

        public ResendEmailService(
            HttpClient httpClient,
            IOptions<EmailSettings> settings,
            ILogger<ResendEmailService> logger)
        {
            _httpClient = httpClient;
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
                    "Email is not configured. Set Email:ApiKey and Email:FromAddress.");
            }

            var from = string.IsNullOrWhiteSpace(_settings.FromName)
                ? _settings.FromAddress
                : $"{_settings.FromName} <{_settings.FromAddress}>";

            var recipient = string.IsNullOrWhiteSpace(toName)
                ? toAddress
                : $"{toName} <{toAddress}>";

            var payload = new
            {
                from,
                to = new[] { recipient },
                subject,
                html = htmlBody,
                text = textBody
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                Endpoint);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            request.Content = JsonContent.Create(payload);

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Resend API failed with status {StatusCode}: {Response}",
                    (int)response.StatusCode,
                    responseBody);

                throw new HttpRequestException(
                    $"Resend API returned {(int)response.StatusCode} ({response.StatusCode}).");
            }

            string? messageId = null;

            try
            {
                using var document = JsonDocument.Parse(responseBody);

                if (document.RootElement.TryGetProperty("id", out var id))
                {
                    messageId = id.GetString();
                }
            }
            catch (JsonException)
            {
                // The email was accepted.
                // The response ID is only used for logging.
            }

            _logger.LogInformation(
                "Email '{Subject}' accepted by Resend for {Address}. MessageId: {MessageId}",
                subject,
                toAddress,
                messageId ?? "unknown");
        }
    }
}