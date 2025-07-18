using System.Net;
using System.Text.Json;
using API.Models;
using API.Exceptions;
using API.Services;

namespace API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _loggingService;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next, 
            ILoggingService loggingService,
            IWebHostEnvironment environment)
        {
            _next = next;
            _loggingService = loggingService;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var errorResponse = CreateErrorResponse(exception, context);
            context.Response.StatusCode = errorResponse.StatusCode;

            // Log del error
            LogException(exception, context, errorResponse);

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private ErrorResponse CreateErrorResponse(Exception exception, HttpContext context)
        {
            var correlationId = context.TraceIdentifier;

            return exception switch
            {
                ValidationException validationEx => new ValidationErrorResponse(validationEx.ValidationErrors.Select(e => 
                    new ValidationError("", e)).ToList())
                {
                    StatusCode = validationEx.StatusCode,
                    ErrorCode = validationEx.ErrorCode,
                    CorrelationId = correlationId,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                AppException appEx => new ErrorResponse
                {
                    StatusCode = appEx.StatusCode,
                    Message = appEx.Message,
                    ErrorCode = appEx.ErrorCode,
                    CorrelationId = correlationId,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                UnauthorizedAccessException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "No tienes permisos para acceder a este recurso.",
                    ErrorCode = "UNAUTHORIZED",
                    CorrelationId = correlationId,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                ArgumentException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = exception.Message,
                    ErrorCode = "INVALID_ARGUMENT",
                    CorrelationId = correlationId,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                InvalidOperationException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = exception.Message,
                    ErrorCode = "INVALID_OPERATION",
                    CorrelationId = correlationId,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                },

                _ => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "Ha ocurrido un error interno del servidor. Por favor, inténtalo de nuevo más tarde.",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    CorrelationId = correlationId,
                    Details = _environment.IsDevelopment() ? GetExceptionDetails(exception) : null
                }
            };
        }

        private void LogException(Exception exception, HttpContext context, ErrorResponse errorResponse)
        {
            var logData = new
            {
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = errorResponse.StatusCode,
                ErrorCode = errorResponse.ErrorCode,
                CorrelationId = errorResponse.CorrelationId,
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                IpAddress = GetClientIpAddress(context)
            };

            if (exception is AppException appEx)
            {
                _loggingService.LogError(exception, 
                    "Error de aplicación: {ErrorCode} - {Message} - Path: {Path} - Method: {Method}", 
                    appEx.ErrorCode, appEx.Message, context.Request.Path, context.Request.Method);
            }
            else if (errorResponse.StatusCode >= 500)
            {
                _loggingService.LogCritical(exception, 
                    "Error crítico del servidor: {Message} - Path: {Path} - Method: {Method}", 
                    exception.Message, context.Request.Path, context.Request.Method);
            }
            else
            {
                _loggingService.LogError(exception, 
                    "Error del cliente: {Message} - Path: {Path} - Method: {Method}", 
                    exception.Message, context.Request.Path, context.Request.Method);
            }
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

        private string GetClientIpAddress(HttpContext context)
        {
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
} 