using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Services;
using API.Exceptions;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstudiantesController : ControllerBase
    {
        private readonly IEstudianteService _service;
        private readonly ILoggingService _loggingService;

        public EstudiantesController(IEstudianteService service, ILoggingService loggingService)
        {
            _service = service;
            _loggingService = loggingService;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registrar([FromBody] EstudianteDto dto)
        {
            try
            {
                _loggingService.LogInformation("Intento de registro de estudiante: {Email}", dto.Correo);
                
                var id = await _service.RegistrarAsync(dto);
                
                _loggingService.LogAudit("ESTUDIANTE_REGISTRO", "system", $"Estudiante registrado con ID: {id}", true);
                _loggingService.LogBusiness("ESTUDIANTE_REGISTRO", "Nuevo estudiante registrado", new { EstudianteId = id, Email = dto.Correo });
                
                return Ok(new { Id = id, Mensaje = "Registro exitoso" });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error en registro de estudiante: {Email}", dto.Correo);
                throw;
            }
        }

        [HttpPost("registro-admin")]
        public async Task<IActionResult> RegistrarAdmin([FromBody] AdminDto dto)
        {
            try
            {
                _loggingService.LogInformation("Intento de registro de administrador: {Email}", dto.Correo);
                
                var id = await _service.RegistrarAdminAsync(dto);
                
                _loggingService.LogAudit("ADMIN_REGISTRO", "system", $"Administrador registrado con ID: {id}", true);
                _loggingService.LogSecurity("ADMIN_REGISTRO", "system", "Nuevo administrador creado", true);
                
                return Ok(new { Id = id, Mensaje = "Administrador registrado exitosamente" });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error en registro de administrador: {Email}", dto.Correo);
                throw;
            }
        }

        [Authorize(Roles = "Estudiante,Admin")]
        [HttpGet("companeromateria/{materiaId}")]
        public async Task<IActionResult> ObtenerCompañeros(int materiaId)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                _loggingService.LogInformation("Consulta de compañeros para materia: {MateriaId}", materiaId);
                
                var nombres = await _service.ObtenerNombresPorMateria(materiaId);
                
                stopwatch.Stop();
                _loggingService.LogPerformance("OBTENER_COMPAÑEROS", stopwatch.ElapsedMilliseconds, $"Materia: {materiaId}");
                
                return Ok(nombres);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo compañeros para materia: {MateriaId}", materiaId);
                throw;
            }
        }

        [Authorize(Roles = "Estudiante")]
        [HttpPost("inscripcion")]
        public async Task<IActionResult> InscribirMaterias([FromBody] InscripcionDto dto)
        {
            try
            {
                // Verificar que el estudiante solo se inscriba a sí mismo
                var estudianteIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (estudianteIdClaim != null && Guid.TryParse(estudianteIdClaim, out var currentUserId))
                {
                    if (currentUserId != dto.EstudianteId)
                    {
                        _loggingService.LogSecurity("INSCRIPCION_NO_AUTORIZADA", currentUserId.ToString(), 
                            $"Intento de inscribir a estudiante {dto.EstudianteId}", false);
                        throw new AuthorizationException("No puedes inscribir a otro estudiante");
                    }
                }

                _loggingService.LogInformation("Inscripción de materias para estudiante: {EstudianteId}", dto.EstudianteId);
                
                await _service.InscribirMateriasAsync(dto);
                
                _loggingService.LogAudit("MATERIAS_INSCRIPCION", dto.EstudianteId.ToString(), 
                    $"Inscrito en {dto.MateriasIds.Count} materias", true);
                _loggingService.LogBusiness("MATERIAS_INSCRIPCION", "Estudiante inscrito en materias", 
                    new { EstudianteId = dto.EstudianteId, Materias = dto.MateriasIds });
                
                return Ok(new { Mensaje = "Inscripción exitosa" });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error en inscripción de materias para estudiante: {EstudianteId}", dto.EstudianteId);
                throw;
            }
        }

        [Authorize(Roles = "Estudiante")]
        [HttpPut("inscripcion")]
        public async Task<IActionResult> ActualizarInscripciones([FromBody] InscripcionDto dto)
        {
            try
            {
                // Verificar que el estudiante solo actualice sus propias inscripciones
                var estudianteIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (estudianteIdClaim != null && Guid.TryParse(estudianteIdClaim, out var currentUserId))
                {
                    if (currentUserId != dto.EstudianteId)
                    {
                        _loggingService.LogSecurity("ACTUALIZACION_INSCRIPCION_NO_AUTORIZADA", currentUserId.ToString(), 
                            $"Intento de actualizar inscripciones de estudiante {dto.EstudianteId}", false);
                        throw new AuthorizationException("No puedes actualizar las inscripciones de otro estudiante");
                    }
                }

                _loggingService.LogInformation("Actualización de inscripciones para estudiante: {EstudianteId}", dto.EstudianteId);
                
                await _service.ActualizarInscripcionesAsync(dto);
                
                _loggingService.LogAudit("INSCRIPCIONES_ACTUALIZACION", dto.EstudianteId.ToString(), 
                    "Inscripciones actualizadas", true);
                
                return Ok(new { Mensaje = "Inscripciones actualizadas exitosamente" });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error actualizando inscripciones para estudiante: {EstudianteId}", dto.EstudianteId);
                throw;
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDto dto)
        {
            try
            {
                _loggingService.LogInformation("Intento de login: {Email}", dto.Correo);
                
                var token = await _service.LoginAsync(dto);
                
                _loggingService.LogSecurity("LOGIN_EXITOSO", "anonymous", $"Login exitoso para: {dto.Correo}", true);
                _loggingService.LogAudit("LOGIN", "anonymous", $"Usuario autenticado: {dto.Correo}", true);
                
                return Ok(token);
            }
            catch (Exception ex)
            {
                _loggingService.LogSecurity("LOGIN_FALLIDO", "anonymous", $"Login fallido para: {dto.Correo}", false);
                _loggingService.LogError(ex, "Error en login: {Email}", dto.Correo);
                throw;
            }
        }

        // Solo para administradores
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/dashboard")]
        public IActionResult DashboardAdmin()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _loggingService.LogAudit("ADMIN_DASHBOARD_ACCESS", userId ?? "unknown", "Acceso al dashboard de administración", true);
            
            return Ok("¡Bienvenido al panel de administración, Admin!");
        }

        // Solo para estudiantes
        [Authorize(Roles = "Estudiante")]
        [HttpGet("estudiante/opciones")]
        public IActionResult OpcionesEstudiante()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _loggingService.LogAudit("ESTUDIANTE_OPCIONES_ACCESS", userId ?? "unknown", "Acceso a opciones de estudiante", true);
            
            return Ok("Acceso a funcionalidades del estudiante");
        }

        [Authorize(Roles = "Estudiante,Admin")]
        [HttpGet("estudiantes")]
        public async Task<IActionResult> ObtenerTodosLosEstudiantes()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                _loggingService.LogInformation("Consulta de todos los estudiantes");
                
                var estudiantes = await _service.ObtenerTodosLosEstudiantesAsync();
                
                stopwatch.Stop();
                _loggingService.LogPerformance("OBTENER_ESTUDIANTES", stopwatch.ElapsedMilliseconds, $"Total: {estudiantes.Count}");
                
                return Ok(estudiantes);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo todos los estudiantes");
                throw;
            }
        }

        [Authorize(Roles = "Estudiante,Admin")]
        [HttpGet("materias")]
        public async Task<IActionResult> ObtenerTodasLasMaterias()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                _loggingService.LogInformation("Consulta de todas las materias");
                
                var materias = await _service.ObtenerTodasLasMateriasAsync();
                
                stopwatch.Stop();
                _loggingService.LogPerformance("OBTENER_MATERIAS", stopwatch.ElapsedMilliseconds, $"Total: {materias.Count}");
                
                return Ok(materias);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo todas las materias");
                throw;
            }
        }

        [Authorize(Roles = "Estudiante,Admin")]
        [HttpGet("profesores")]
        public async Task<IActionResult> ObtenerTodosLosProfesores()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                _loggingService.LogInformation("Consulta de todos los profesores");
                
                var profesores = await _service.ObtenerTodosLosProfesoresAsync();
                
                stopwatch.Stop();
                _loggingService.LogPerformance("OBTENER_PROFESORES", stopwatch.ElapsedMilliseconds, $"Total: {profesores.Count}");
                
                return Ok(profesores);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo todos los profesores");
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("inscripciones")]
        public async Task<IActionResult> ObtenerTodasLasInscripciones()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                _loggingService.LogInformation("Consulta de todas las inscripciones");
                
                var inscripciones = await _service.ObtenerTodasLasInscripcionesAsync();
                
                stopwatch.Stop();
                _loggingService.LogPerformance("OBTENER_INSCRIPCIONES", stopwatch.ElapsedMilliseconds, $"Total: {inscripciones.Count}");
                
                return Ok(inscripciones);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo todas las inscripciones");
                throw;
            }
        }

        // Endpoints específicos para estudiantes
        [Authorize(Roles = "Estudiante")]
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> ObtenerMiPerfil()
        {
            try
            {
                var estudianteIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (estudianteIdClaim == null || !Guid.TryParse(estudianteIdClaim, out var estudianteId))
                {
                    _loggingService.LogSecurity("PERFIL_NO_AUTORIZADO", "unknown", "Intento de acceso sin autenticación", false);
                    throw new AuthenticationException("No autenticado");
                }

                _loggingService.LogInformation("Consulta de perfil para estudiante: {EstudianteId}", estudianteId);
                
                var perfil = await _service.ObtenerPerfilEstudianteAsync(estudianteId);
                
                _loggingService.LogAudit("PERFIL_CONSULTA", estudianteId.ToString(), "Perfil consultado", true);
                
                return Ok(perfil);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo perfil de estudiante");
                throw;
            }
        }

        [Authorize(Roles = "Estudiante")]
        [HttpGet("mis-inscripciones")]
        public async Task<IActionResult> ObtenerMisInscripciones()
        {
            try
            {
                var estudianteIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (estudianteIdClaim == null || !Guid.TryParse(estudianteIdClaim, out var estudianteId))
                {
                    _loggingService.LogSecurity("INSCRIPCIONES_NO_AUTORIZADO", "unknown", "Intento de acceso sin autenticación", false);
                    throw new AuthenticationException("No autenticado");
                }

                _loggingService.LogInformation("Consulta de inscripciones para estudiante: {EstudianteId}", estudianteId);
                
                var inscripciones = await _service.ObtenerInscripcionesEstudianteAsync(estudianteId);
                
                _loggingService.LogAudit("INSCRIPCIONES_CONSULTA", estudianteId.ToString(), 
                    $"Consultadas {inscripciones.Count} inscripciones", true);
                
                return Ok(inscripciones);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo inscripciones de estudiante");
                throw;
            }
        }

        [Authorize(Roles = "Estudiante")]
        [HttpGet("mis-compañeros/{materiaId}")]
        public async Task<IActionResult> ObtenerMisCompañeros(int materiaId)
        {
            try
            {
                var estudianteIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (estudianteIdClaim == null || !Guid.TryParse(estudianteIdClaim, out var estudianteId))
                {
                    _loggingService.LogSecurity("COMPAÑEROS_NO_AUTORIZADO", "unknown", "Intento de acceso sin autenticación", false);
                    throw new AuthenticationException("No autenticado");
                }

                _loggingService.LogInformation("Consulta de compañeros para estudiante: {EstudianteId} en materia: {MateriaId}", 
                    estudianteId, materiaId);

                // Verificar que el estudiante esté inscrito en la materia
                var inscripciones = await _service.ObtenerInscripcionesEstudianteAsync(estudianteId);
                if (!inscripciones.Any(i => i.MateriaId == materiaId))
                {
                    _loggingService.LogSecurity("COMPAÑEROS_NO_INSCRITO", estudianteId.ToString(), 
                        $"Intento de ver compañeros sin estar inscrito en materia {materiaId}", false);
                    throw new AuthorizationException("No estás inscrito en esta materia");
                }

                var compañeros = await _service.ObtenerCompañerosPorMateriaAsync(materiaId, estudianteId);
                
                _loggingService.LogAudit("COMPAÑEROS_CONSULTA", estudianteId.ToString(), 
                    $"Consultados {compañeros.Count} compañeros en materia {materiaId}", true);
                
                return Ok(compañeros);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error obteniendo compañeros para estudiante en materia: {MateriaId}", materiaId);
                throw;
            }
        }

        // Endpoint temporal para verificar/crear administrador
        [HttpGet("check-admin")]
        public async Task<IActionResult> CheckAdmin()
        {
            try
            {
                _loggingService.LogInformation("Verificación de administrador");
                
                var estudiantes = await _service.ObtenerTodosLosEstudiantesAsync();
                var admin = estudiantes.FirstOrDefault(e => e.Correo == "admin@admin.com");
                
                if (admin == null)
                {
                    // Crear administrador si no existe
                    var adminDto = new AdminDto
                    {
                        Nombre = "Administrador",
                        Correo = "admin@admin.com",
                        Password = "Contraseña2025*",
                        CodigoAdmin = "ADMIN2024"
                    };
                    
                    var adminId = await _service.RegistrarAdminAsync(adminDto);
                    
                    _loggingService.LogAudit("ADMIN_CREACION", "system", "Administrador creado automáticamente", true);
                    _loggingService.LogSecurity("ADMIN_CREACION", "system", "Nuevo administrador creado", true);
                    
                    return Ok(new { 
                        Mensaje = "Administrador creado exitosamente",
                        AdminId = adminId,
                        Credenciales = new { 
                            Correo = "admin@admin.com", 
                            Password = "Contraseña2025*" 
                        }
                    });
                }
                
                _loggingService.LogInformation("Administrador ya existe: {AdminId}", admin.Id);
                
                return Ok(new { 
                    Mensaje = "Administrador ya existe",
                    Admin = new { 
                        Id = admin.Id, 
                        Nombre = admin.Nombre, 
                        Correo = admin.Correo,
                        Rol = admin.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error verificando/creando administrador");
                throw;
            }
        }
    }
}
