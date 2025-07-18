using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    /// <summary>
    /// Modelo estandarizado para respuestas de error
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Código de estado HTTP
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Mensaje de error para el usuario
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Código de error interno para debugging
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Detalles adicionales del error (solo en desarrollo)
        /// </summary>
        public object? Details { get; set; }

        /// <summary>
        /// Timestamp del error
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID de correlación para tracking
        /// </summary>
        public string? CorrelationId { get; set; }

        public ErrorResponse()
        {
        }

        public ErrorResponse(int statusCode, string message, string? errorCode = null, object? details = null)
        {
            StatusCode = statusCode;
            Message = message;
            ErrorCode = errorCode;
            Details = details;
        }
    }

    /// <summary>
    /// Modelo para errores de validación
    /// </summary>
    public class ValidationErrorResponse : ErrorResponse
    {
        /// <summary>
        /// Lista de errores de validación
        /// </summary>
        public List<ValidationError> ValidationErrors { get; set; } = new();

        public ValidationErrorResponse() : base(400, "Error de validación")
        {
        }

        public ValidationErrorResponse(List<ValidationError> validationErrors) 
            : base(400, "Error de validación")
        {
            ValidationErrors = validationErrors;
        }
    }

    /// <summary>
    /// Error de validación individual
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Campo que falló la validación
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje de error de validación
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Valor que causó el error
        /// </summary>
        public object? Value { get; set; }

        public ValidationError()
        {
        }

        public ValidationError(string field, string message, object? value = null)
        {
            Field = field;
            Message = message;
            Value = value;
        }
    }
} 