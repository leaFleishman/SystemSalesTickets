using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SystemSalesTickets.Api.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CatchErorrsMiddleware
    {
        private readonly RequestDelegate _next;

        public CatchErorrsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);

            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync($"there was a problem try again: {ex.Message}");
            }



        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CheckIfBuyMiddlewareExtensions
    {
        public static IApplicationBuilder UseCheckIfBuyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CatchErorrsMiddleware>();
        }
    }
}
