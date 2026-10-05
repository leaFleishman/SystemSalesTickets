using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Settings;

namespace SystemSalesTickets.Service.Service
{
    public sealed class GoogleAppsScriptEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly EmailSettings _settings;
        private readonly ILogger<GoogleAppsScriptEmailService> _logger;

        public GoogleAppsScriptEmailService(
            HttpClient httpClient,
            IOptions<EmailSettings> settings,
            ILogger<GoogleAppsScriptEmailService> logger)
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
                    "Email is not configured. Set Email:ApiUrl and Email:Secret.");
            }

            var payload = new
            {
                secret = _settings.Secret,
                to = toAddress,
                subject,
                htmlBody,
                textBody = textBody ?? string.Empty
            };

            using var response = await _httpClient.PostAsJsonAsync(
                _settings.ApiUrl,
                payload,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Google Apps Script email gateway failed with status {StatusCode}: {Response}",
                    (int)response.StatusCode,
                    responseBody);

                throw new HttpRequestException(
                    $"Google Apps Script email gateway returned {(int)response.StatusCode} ({response.StatusCode}).");
            }

            _logger.LogInformation(
                "Email '{Subject}' accepted by Google Apps Script for {Address}",
                subject,
                toAddress);
        }
    }
}