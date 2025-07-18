namespace API.Exceptions
{
    /// <summary>
    /// Excepción base para errores de la aplicación
    /// </summary>
    public class AppException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        public AppException(string message, string errorCode = "APP_ERROR", int statusCode = 500) 
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public AppException(string message, Exception innerException, string errorCode = "APP_ERROR", int statusCode = 500) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Excepción para errores de validación
    /// </summary>
    public class ValidationException : AppException
    {
        public List<string> ValidationErrors { get; }

        public ValidationException(string message, List<string>? validationErrors = null) 
            : base(message, "VALIDATION_ERROR", 400)
        {
            ValidationErrors = validationErrors ?? new List<string>();
        }
    }

    /// <summary>
    /// Excepción para recursos no encontrados
    /// </summary>
    public class NotFoundException : AppException
    {
        public string? ResourceName { get; }
        public object? ResourceId { get; }

        public NotFoundException(string resourceName, object resourceId) 
            : base($"El recurso '{resourceName}' con ID '{resourceId}' no fue encontrado.", "NOT_FOUND", 404)
        {
            ResourceName = resourceName;
            ResourceId = resourceId;
        }

        public NotFoundException(string message) 
            : base(message, "NOT_FOUND", 404)
        {
        }
    }

    /// <summary>
    /// Excepción para errores de autenticación
    /// </summary>
    public class AuthenticationException : AppException
    {
        public AuthenticationException(string message) 
            : base(message, "AUTHENTICATION_ERROR", 401)
        {
        }
    }

    /// <summary>
    /// Excepción para errores de autorización
    /// </summary>
    public class AuthorizationException : AppException
    {
        public AuthorizationException(string message) 
            : base(message, "AUTHORIZATION_ERROR", 403)
        {
        }
    }

    /// <summary>
    /// Excepción para conflictos de datos
    /// </summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message) 
            : base(message, "CONFLICT_ERROR", 409)
        {
        }
    }

    /// <summary>
    /// Excepción para errores de base de datos
    /// </summary>
    public class DatabaseException : AppException
    {
        public DatabaseException(string message, Exception? innerException = null) 
            : base(message, innerException ?? new Exception("Unknown database error"), "DATABASE_ERROR", 500)
        {
        }
    }

    /// <summary>
    /// Excepción para errores de configuración
    /// </summary>
    public class ConfigurationException : AppException
    {
        public ConfigurationException(string message) 
            : base(message, "CONFIGURATION_ERROR", 500)
        {
        }
    }
} 