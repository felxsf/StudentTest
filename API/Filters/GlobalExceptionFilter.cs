using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using API.Services;
using API.Exceptions;
using API.Models;

namespace API.Filters
{
    /// <summary>
    /// Filtro global para manejar excepciones en controladores
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILoggingService _loggingService;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionFilter(ILoggingService loggingService, IWebHostEnvironment environment)
        {
            _loggingService = loggingService;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";

            // Log del error
            _loggingService.LogError(exception, 
                "Error en controlador: {Controller}.{Action} - {Message}", 
                controllerName, actionName, exception.Message);

            // Crear respuesta de error
            var errorResponse = CreateErrorResponse(exception);

            // Configurar resultado
            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = errorResponse.StatusCode
            };

            // Marcar como manejado
            context.ExceptionHandled = true;
        }

        private ErrorResponse CreateErrorResponse(Exception exception)
        {
            return exception switch
            {
                AppException appEx => new ErrorResponse
                {
                    StatusCode = appEx.StatusCode,
                    Message = appEx.Message,
                    ErrorCode = appEx.ErrorCode,
                    Timestamp = DateTime.UtcNow,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                UnauthorizedAccessException => new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "No tienes permisos para acceder a este recurso.",
                    ErrorCode = "UNAUTHORIZED",
                    Timestamp = DateTime.UtcNow,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                ArgumentException => new ErrorResponse
                {
                    StatusCode = 400,
                    Message = exception.Message,
                    ErrorCode = "INVALID_ARGUMENT",
                    Timestamp = DateTime.UtcNow,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                InvalidOperationException => new ErrorResponse
                {
                    StatusCode = 400,
                    Message = exception.Message,
                    ErrorCode = "INVALID_OPERATION",
                    Timestamp = DateTime.UtcNow,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                _ => new ErrorResponse
                {
                    StatusCode = 500,
                    Message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "Ha ocurrido un error interno del servidor. Por favor, inténtalo de nuevo más tarde.",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Timestamp = DateTime.UtcNow,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                }
            };
        }

        private object GetExceptionDetails(Exception exception)
        {
            return new
            {
                ExceptionType = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException != null ? new
                {
                    ExceptionType = exception.InnerException.GetType().Name,
                    Message = exception.InnerException.Message
                } : null
            };
        }
    }
} 