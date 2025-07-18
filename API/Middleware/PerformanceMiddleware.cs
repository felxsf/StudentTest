using API.Services;

namespace API.Middleware
{
    /// <summary>
    /// Middleware para medir el rendimiento de las peticiones HTTP
    /// </summary>
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _loggingService;

        public PerformanceMiddleware(RequestDelegate next, ILoggingService loggingService)
        {
            _next = next;
            _loggingService = loggingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context);

                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);

                stopwatch.Stop();
                
                // Log de rendimiento solo para peticiones que tomen más de 100ms
                if (stopwatch.ElapsedMilliseconds > 100)
                {
                    _loggingService.LogPerformance(
                        "HTTP_REQUEST", 
                        stopwatch.ElapsedMilliseconds, 
                        $"{context.Request.Method} {context.Request.Path} - Status: {context.Response.StatusCode}"
                    );
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Log de error con información de rendimiento
                _loggingService.LogError(ex, 
                    "Error en petición HTTP: {Method} {Path} - Duration: {Duration}ms - Status: {StatusCode}", 
                    context.Request.Method, 
                    context.Request.Path, 
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
                
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
} 