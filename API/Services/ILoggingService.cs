namespace API.Services
{
    /// <summary>
    /// Interfaz para el servicio de logging personalizado
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Registra un mensaje de información
        /// </summary>
        void LogInformation(string message, params object[] args);

        /// <summary>
        /// Registra un mensaje de advertencia
        /// </summary>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Registra un error
        /// </summary>
        void LogError(Exception exception, string message, params object[] args);

        /// <summary>
        /// Registra un error crítico
        /// </summary>
        void LogCritical(Exception exception, string message, params object[] args);

        /// <summary>
        /// Registra un mensaje de debug
        /// </summary>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// Registra información de auditoría
        /// </summary>
        void LogAudit(string action, string userId, string details, bool success = true);

        /// <summary>
        /// Registra información de seguridad
        /// </summary>
        void LogSecurity(string action, string userId, string details, bool success = true);

        /// <summary>
        /// Registra información de rendimiento
        /// </summary>
        void LogPerformance(string operation, long durationMs, string details = "");

        /// <summary>
        /// Registra información de negocio
        /// </summary>
        void LogBusiness(string operation, string details, object? data = null);
    }
} 