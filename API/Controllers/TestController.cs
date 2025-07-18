using Microsoft.AspNetCore.Mvc;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { 
                Status = "OK", 
                Timestamp = DateTime.UtcNow,
                Message = "API is running"
            });
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Probar conexión a la base de datos
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(500, new { 
                        Status = "ERROR", 
                        Message = "Cannot connect to database",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Probar consulta simple
                var logCount = await _context.Logs.CountAsync();
                
                return Ok(new { 
                    Status = "OK", 
                    Message = "Database connection successful",
                    LogCount = logCount,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Status = "ERROR", 
                    Message = ex.Message,
                    Exception = ex.GetType().Name,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("logs-count")]
        public async Task<IActionResult> GetLogsCount()
        {
            try
            {
                var count = await _context.Logs.CountAsync();
                return Ok(new { Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = ex.Message,
                    Exception = ex.GetType().Name
                });
            }
        }
    }
} 