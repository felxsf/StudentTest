using Serilog;
using Serilog.Events;

namespace API.Services
{
    /// <summary>
    /// Implementación del servicio de logging personalizado
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly Serilog.ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingService(Serilog.ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.Information(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.Warning(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            var enrichedMessage = EnrichMessage(message);
            _logger.Error(exception, enrichedMessage, args);
        }

        public void LogCritical(Exception exception, string message, params object[] args)
        {
            var enrichedMessage = EnrichMessage(message);
            _logger.Fatal(exception, enrichedMessage, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        public void LogAudit(string action, string userId, string details, bool success = true)
        {
            var auditData = new
            {
                Action = action,
                UserId = userId,
                Details = details,
                Success = success,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _logger.Information("AUDIT: {Action} by {UserId} - {Details} - Success: {Success}", 
                action, userId, details, success);
        }

        public void LogSecurity(string action, string userId, string details, bool success = true)
        {
            var securityData = new
            {
                Action = action,
                UserId = userId,
                Details = details,
                Success = success,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            var logLevel = success ? LogEventLevel.Information : LogEventLevel.Warning;
            _logger.Write(logLevel, "SECURITY: {Action} by {UserId} - {Details} - Success: {Success}", 
                action, userId, details, success);
        }

        public void LogPerformance(string operation, long durationMs, string details = "")
        {
            var performanceData = new
            {
                Operation = operation,
                DurationMs = durationMs,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            var logLevel = durationMs > 1000 ? LogEventLevel.Warning : LogEventLevel.Information;
            _logger.Write(logLevel, "PERFORMANCE: {Operation} took {DurationMs}ms - {Details}", 
                operation, durationMs, details);
        }

        public void LogBusiness(string operation, string details, object? data = null)
        {
            var businessData = new
            {
                Operation = operation,
                Details = details,
                Data = data,
                Timestamp = DateTime.UtcNow,
                UserId = GetCurrentUserId()
            };

            _logger.Information("BUSINESS: {Operation} - {Details} - Data: {@Data}", 
                operation, details, data);
        }

        private string EnrichMessage(string message)
        {
            var correlationId = GetCorrelationId();
            var userId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();

            return $"[{correlationId}] [{userId}] [{ipAddress}] {message}";
        }

        private string GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.TraceIdentifier ?? "unknown";
        }

        private string GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true 
                ? user.Identity.Name ?? "anonymous" 
                : "anonymous";
        }

        private string GetClientIpAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return "unknown";

            // Obtener IP del cliente considerando proxies
            var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        }
    }
} 