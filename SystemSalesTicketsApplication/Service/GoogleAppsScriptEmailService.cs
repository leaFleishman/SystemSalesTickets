using System.Diagnostics;
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

            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Starting Google Apps Script email request for {Address}, subject '{Subject}'",
                toAddress,
                subject);

            using var timeoutCts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                using var response = await _httpClient.PostAsJsonAsync(
                    _settings.ApiUrl,
                    payload,
                    timeoutCts.Token);

                var responseBody =
                    await response.Content.ReadAsStringAsync(timeoutCts.Token);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Google Apps Script email request finished in {ElapsedMs} ms with status {StatusCode}: {Response}",
                    stopwatch.ElapsedMilliseconds,
                    (int)response.StatusCode,
                    responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Google Apps Script email gateway returned {(int)response.StatusCode} ({response.StatusCode}).");
                }
            }
            catch (OperationCanceledException) when (
                timeoutCts.IsCancellationRequested &&
                !cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                _logger.LogError(
                    "Google Apps Script email request timed out after {ElapsedMs} ms for {Address}",
                    stopwatch.ElapsedMilliseconds,
                    toAddress);

                throw new TimeoutException(
                    "Google Apps Script email request timed out after 10 seconds.");
            }
        }
    }
}