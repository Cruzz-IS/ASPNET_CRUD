using WebApiRRHH.Context;
using WebApiRRHH.Models;

namespace WebApiRRHH.Services.Auth
{
    public interface IAuditService
    {

        Task LogAsync(
            string action,
            string entityType,
            int? entityId,
            string? oldValues,
            string? newValues,
            string? ipAddress,
            string? userAgent,
            int? userId,
            string severity = "Info");
    }

    public class AuditService : IAuditService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AppDBContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(
            string action,
            string entityType,
            int? entityId,
            string? oldValues,
            string? newValues,
            string? ipAddress,
            string? userAgent,
            int? userId,
            string severity = "Info")
        {
            try
            {
                var auditLog = new Audit
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId?.ToString(),
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    UserId = userId,
                    Severity = severity,
                    Timestamp = DateTime.UtcNow
                };

                _context.Audits.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // No dejar que un error en auditoría detenga el flujo principal de la API
                _logger.LogError(ex, "Error al guardar registro de auditoría");
            }
        }
    }
}