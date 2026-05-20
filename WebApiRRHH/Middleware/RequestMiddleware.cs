using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;

namespace WebApiRRHH.Middleware
{
    /// <summary>
    /// Middleware para logging detallado de todas las requests HTTP
    /// Registra: método, ruta, duración, status code, IP
    /// </summary>
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMiddleware> _logger;

        public RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger) => (_next, _logger) = (next, logger);

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var method = request.Method;
            var path = request.Path;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

            _logger.LogInformation("HTTP {Method} {Path} - User: {UserId} - IP: {IpAddress}",
                method, path, userId, ipAddress);

            try
            {
                await _next(context);
                stopwatch.Stop();

                _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "HTTP {Method} {Path} failed", method, path);
                throw;
            }
        }

        //private string GetIpAddress(HttpContext context)
        //{
        //    var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        //    if (string.IsNullOrEmpty(ipAddress))
        //    {
        //        ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        //    }
        //    return ipAddress;
        //}
    }

    // Extension method
    public static class RequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestMiddleware>();
        }
    }
}