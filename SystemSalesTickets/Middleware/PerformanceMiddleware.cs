using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SystemSalesTickets.API.Middleware
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;

        // סף (במילישניות) שמעליו נחשב "בקשה איטית"
        private const int SlowRequestThresholdMs = 1000;

        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            var method = context.Request.Method;
            var path = context.Request.Path;
            var statusCode = context.Response.StatusCode;

            if (elapsedMs >= SlowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "SLOW request: {Method} {Path} took {ElapsedMs}ms (status {StatusCode})",
                    method, path, elapsedMs, statusCode);
            }
            else
            {
                _logger.LogInformation(
                    "{Method} {Path} completed in {ElapsedMs}ms (status {StatusCode})",
                    method, path, elapsedMs, statusCode);
            }
        }
    }

    // פונקציית הרחבה לרישום נקי ב-Program.cs
    public static class PerformanceMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceMiddleware>();
        }
    }
}