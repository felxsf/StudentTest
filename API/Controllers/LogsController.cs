using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Context;
using API.Services;
using API.Exceptions;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILoggingService _loggingService;

        public LogsController(AppDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("LOGS_DASHBOARD_ACCESS", userId ?? "unknown", "Acceso al dashboard de logs", true);

                // Estadísticas generales
                var totalLogs = await _context.Logs.CountAsync();
                var todayLogs = await _context.Logs
                    .Where(l => l.TimeStamp.Date == DateTime.Today)
                    .CountAsync();

                var errorLogs = await _context.Logs
                    .Where(l => l.Level == "Error" || l.Level == "Fatal")
                    .CountAsync();

                var todayErrors = await _context.Logs
                    .Where(l => (l.Level == "Error" || l.Level == "Fatal") && l.TimeStamp.Date == DateTime.Today)
                    .CountAsync();

                // Logs por nivel
                var logsByLevel = await _context.Logs
                    .GroupBy(l => l.Level)
                    .Select(g => new { Level = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Logs por hora (últimas 24 horas)
                var hourlyLogs = await _context.Logs
                    .Where(l => l.TimeStamp >= DateTime.Today.AddDays(-1))
                    .GroupBy(l => new { Hour = l.TimeStamp.Hour, Date = l.TimeStamp.Date })
                    .Select(g => new { 
                        Date = g.Key.Date, 
                        Hour = g.Key.Hour, 
                        Count = g.Count(),
                        Errors = g.Count(l => l.Level == "Error" || l.Level == "Fatal")
                    })
                    .OrderBy(x => x.Date)
                    .ThenBy(x => x.Hour)
                    .ToListAsync();

                var stats = new
                {
                    TotalLogs = totalLogs,
                    TodayLogs = todayLogs,
                    TotalErrors = errorLogs,
                    TodayErrors = todayErrors,
                    LogsByLevel = logsByLevel,
                    HourlyLogs = hourlyLogs,
                    LastUpdated = DateTime.Now
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo estadísticas del dashboard de logs");
                throw;
            }
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("LOGS_RECENT_ACCESS", userId ?? "unknown", $"Consulta de logs recientes - Página {page}", true);

                var query = _context.Logs.AsQueryable();

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var logs = await query
                    .OrderByDescending(l => l.TimeStamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.TimeStamp,
                        l.Level,
                        l.Message,
                        l.Exception,
                        l.UserId,
                        l.Properties
                    })
                    .ToListAsync();

                var result = new
                {
                    Logs = logs,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo logs recientes");
                throw;
            }
        }

        [HttpGet("errors")]
        public async Task<IActionResult> GetErrorLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("LOGS_ERRORS_ACCESS", userId ?? "unknown", $"Consulta de logs de errores - Página {page}", true);

                var query = _context.Logs
                    .Where(l => l.Level == "Error" || l.Level == "Fatal");

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var logs = await query
                    .OrderByDescending(l => l.TimeStamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.TimeStamp,
                        l.Level,
                        l.Message,
                        l.Exception,
                        l.UserId,
                        l.Properties
                    })
                    .ToListAsync();

                var result = new
                {
                    Logs = logs,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo logs de errores");
                throw;
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchLogs(
            [FromQuery] string? level = null,
            [FromQuery] string? message = null,
            [FromQuery] string? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("LOGS_SEARCH", currentUserId ?? "unknown", 
                    $"Búsqueda de logs - Level: {level}, Message: {message}, UserId: {userId}", true);

                var query = _context.Logs.AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(l => l.Level.Contains(level));
                }

                if (!string.IsNullOrEmpty(message))
                {
                    query = query.Where(l => l.Message.Contains(message));
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(l => l.UserId != null && l.UserId.Contains(userId));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.TimeStamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(l => l.TimeStamp <= endDate.Value);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var logs = await query
                    .OrderByDescending(l => l.TimeStamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.TimeStamp,
                        l.Level,
                        l.Message,
                        l.Exception,
                        l.UserId,
                        l.Properties
                    })
                    .ToListAsync();

                var result = new
                {
                    Logs = logs,
                    Filters = new
                    {
                        Level = level,
                        Message = message,
                        UserId = userId,
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error en búsqueda de logs");
                throw;
            }
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("LOGS_PERFORMANCE_ACCESS", userId ?? "unknown", $"Consulta de logs de rendimiento - Página {page}", true);

                var query = _context.Logs
                    .Where(l => l.Message.Contains("Performance") || l.Message.Contains("slow"));

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var logs = await query
                    .OrderByDescending(l => l.TimeStamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.TimeStamp,
                        l.Level,
                        l.Message,
                        l.Exception,
                        l.UserId,
                        l.Properties
                    })
                    .ToListAsync();

                var result = new
                {
                    Logs = logs,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo logs de rendimiento");
                throw;
            }
        }

        [HttpGet("security")]
        public async Task<IActionResult> GetSecurityLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("LOGS_SECURITY_ACCESS", userId ?? "unknown", $"Consulta de logs de seguridad - Página {page}", true);

                var query = _context.Logs
                    .Where(l => l.Message.Contains("Security") || 
                               l.Message.Contains("LOGIN") || 
                               l.Message.Contains("AUTH") ||
                               l.Message.Contains("INSCRIPCION_NO_AUTORIZADA") ||
                               l.Message.Contains("PERFIL_NO_AUTORIZADO"));

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var logs = await query
                    .OrderByDescending(l => l.TimeStamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.TimeStamp,
                        l.Level,
                        l.Message,
                        l.Exception,
                        l.UserId,
                        l.Properties
                    })
                    .ToListAsync();

                var result = new
                {
                    Logs = logs,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo logs de seguridad");
                throw;
            }
        }

        [HttpDelete("cleanup")]
        public async Task<IActionResult> CleanupOldLogs([FromQuery] int daysToKeep = 30)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("LOGS_CLEANUP", userId ?? "unknown", $"Inicio de limpieza de logs - Mantener {daysToKeep} días", true);

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var logsToDelete = await _context.Logs
                    .Where(l => l.TimeStamp < cutoffDate)
                    .CountAsync();

                if (logsToDelete > 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "DELETE FROM Logs WHERE TimeStamp < {0}", cutoffDate);

                    _loggingService.LogAudit("LOGS_CLEANUP_COMPLETED", userId ?? "unknown", 
                        $"Limpieza completada - {logsToDelete} logs eliminados", true);

                    return Ok(new { 
                        Message = $"Limpieza completada. {logsToDelete} logs eliminados.",
                        DeletedCount = logsToDelete,
                        CutoffDate = cutoffDate
                    });
                }

                return Ok(new { 
                    Message = "No hay logs antiguos para eliminar.",
                    DeletedCount = 0,
                    CutoffDate = cutoffDate
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error en limpieza de logs");
                throw;
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportLogs(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? level = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _loggingService.LogAudit("LOGS_EXPORT", userId ?? "unknown", 
                    $"Exportación de logs - Start: {startDate}, End: {endDate}, Level: {level}", true);

                var query = _context.Logs.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.TimeStamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(l => l.TimeStamp <= endDate.Value);
                }

                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(l => l.Level == level);
                }

                var logs = await query
                    .OrderByDescending(l => l.TimeStamp)
                    .Select(l => new
                    {
                        l.Id,
                        l.TimeStamp,
                        l.Level,
                        l.Message,
                        l.Exception,
                        l.UserId,
                        l.Properties
                    })
                    .ToListAsync();

                // Convertir a CSV
                var csvContent = "Id,TimeStamp,Level,Message,Exception,UserId,Properties\n";
                
                foreach (var log in logs)
                {
                    csvContent += $"{log.Id},{log.TimeStamp:yyyy-MM-dd HH:mm:ss},\"{log.Level}\",\"{log.Message?.Replace("\"", "\"\"")}\",\"{log.Exception?.Replace("\"", "\"\"")}\",\"{log.UserId}\",\"{log.Properties}\"\n";
                }

                var fileName = $"logs_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(
                    System.Text.Encoding.UTF8.GetBytes(csvContent),
                    "text/csv",
                    fileName);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error en exportación de logs");
                throw;
            }
        }
    }
} 