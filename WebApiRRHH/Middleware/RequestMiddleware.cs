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

        public RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Obtener información de la request
            var request = context.Request;
            var method = request.Method;
            var path = request.Path;
            var queryString = request.QueryString;
            var ipAddress = GetIpAddress(context);
            var empleadoAgent = request.Headers["Empleado-Agent"].ToString();
            var empleadoId = context.User?.FindFirst("sub")?.Value ?? "Anonymous";

            // Log de request entrante de parte del cliente
            _logger.LogInformation(
                "HTTP {Method} {Path}{QueryString} - Empleado: {EmpleadoId} - IP: {IpAddress}",
                method, path, queryString, empleadoId, ipAddress);

            // Capturar la respuesta 
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // Ejecutar el siguiente middleware
                await _next(context);

                stopwatch.Stop();

                // Log de la respuesta que se le muestra al cliente
                var statusCode = context.Response.StatusCode;
                var level = statusCode >= 500 ? LogLevel.Error :
                           statusCode >= 400 ? LogLevel.Warning :
                           LogLevel.Information;

                _logger.Log(level,
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms - Empleado: {EmpleadoId}",
                    method, path, statusCode, stopwatch.ElapsedMilliseconds, empleadoId);

                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "HTTP {Method} {Path} failed after {ElapsedMs}ms - Empleado: {EmpleadoId} - Error: {Error}",
                    method, path, stopwatch.ElapsedMilliseconds, empleadoId, ex.Message);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private string GetIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            return ipAddress;
        }
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