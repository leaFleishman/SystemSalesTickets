using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Settings;

namespace SystemSalesTickets.Service.Service
{
    public sealed class BrevoEmailService : IEmailService
    {
        private const string Endpoint = "https://api.brevo.com/v3/smtp/email";

        private readonly HttpClient _httpClient;
        private readonly EmailSettings _settings;
        private readonly ILogger<BrevoEmailService> _logger;

        public BrevoEmailService(
            HttpClient httpClient,
            IOptions<EmailSettings> settings,
            ILogger<BrevoEmailService> logger)
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

            var payload = new
            {
                sender = new
                {
                    name = _settings.FromName,
                    email = _settings.FromAddress
                },
                to = new[]
                {
                    new
                    {
                        email = toAddress,
                        name = toName ?? string.Empty
                    }
                },
                subject,
                htmlContent = htmlBody,
                textContent = textBody
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Headers.Add("api-key", _settings.ApiKey);
            request.Content = JsonContent.Create(payload);

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Brevo API failed with status {StatusCode}: {Response}",
                    (int)response.StatusCode,
                    responseBody);

                throw new HttpRequestException(
                    $"Brevo API returned {(int)response.StatusCode} ({response.StatusCode}).");
            }

            _logger.LogInformation(
                "Email '{Subject}' accepted by Brevo for {Address}",
                subject,
                toAddress);
        }
    }
}