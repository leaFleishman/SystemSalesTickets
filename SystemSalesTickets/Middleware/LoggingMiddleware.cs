using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SystemSalesTickets.API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(
            RequestDelegate next,
            ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = Guid.NewGuid().ToString();

            context.Items["CorrelationId"] = correlationId;

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
            {
                _logger.LogInformation(
                    "Incoming request: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unhandled exception while processing request");

                    throw;
                }
            }
        }
    }
}
