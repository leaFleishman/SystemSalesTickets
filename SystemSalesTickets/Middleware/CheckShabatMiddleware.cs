using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SystemSalesTickets.Api.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CheckShabatMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckShabatMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (DateTime.Now.DayOfWeek != DayOfWeek.Saturday)
            {
                await _next(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await httpContext.Response.WriteAsync("the system is closed in shabbat kodesh!");
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CheckShabatMiddlewareExtensions
    {
        public static IApplicationBuilder UseCheckShabatMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CheckShabatMiddleware>();
        }
    }
}
