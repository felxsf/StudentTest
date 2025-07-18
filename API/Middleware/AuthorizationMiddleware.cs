using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    Error = "No autorizado",
                    Message = ex.Message,
                    StatusCode = 401
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    Error = "Operación inválida",
                    Message = ex.Message,
                    StatusCode = 400
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
} 