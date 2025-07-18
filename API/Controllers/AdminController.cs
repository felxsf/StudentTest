using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IEstudianteService _service;

        public AdminController(IEstudianteService service)
        {
            _service = service;
        }

        // Gestión de Estudiantes
        [HttpGet("estudiantes")]
        public async Task<IActionResult> ObtenerEstudiantes()
        {
            var estudiantes = await _service.ObtenerTodosLosEstudiantesAsync();
            return Ok(estudiantes);
        }

        [HttpDelete("estudiantes/{estudianteId}")]
        public async Task<IActionResult> EliminarEstudiante(Guid estudianteId)
        {
            var resultado = await _service.EliminarEstudianteAsync(estudianteId);
            if (!resultado)
                return NotFound("Estudiante no encontrado");

            return Ok(new { Mensaje = "Estudiante eliminado exitosamente" });
        }

        // Gestión de Profesores
        [HttpGet("profesores")]
        public async Task<IActionResult> ObtenerProfesores()
        {
            var profesores = await _service.ObtenerTodosLosProfesoresAsync();
            return Ok(profesores);
        }

        [HttpPost("profesores")]
        public async Task<IActionResult> AgregarProfesor([FromBody] ProfesorDto dto)
        {
            var profesor = await _service.AgregarProfesorAsync(dto);
            return CreatedAtAction(nameof(ObtenerProfesores), new { id = profesor.Id }, profesor);
        }

        [HttpPut("profesores/{profesorId}")]
        public async Task<IActionResult> ActualizarProfesor(int profesorId, [FromBody] ProfesorDto dto)
        {
            var profesor = await _service.ActualizarProfesorAsync(profesorId, dto);
            return Ok(profesor);
        }

        [HttpDelete("profesores/{profesorId}")]
        public async Task<IActionResult> EliminarProfesor(int profesorId)
        {
            var resultado = await _service.EliminarProfesorAsync(profesorId);
            if (!resultado)
                return NotFound("Profesor no encontrado");

            return Ok(new { Mensaje = "Profesor eliminado exitosamente" });
        }

        // Gestión de Materias
        [HttpGet("materias")]
        public async Task<IActionResult> ObtenerMaterias()
        {
            var materias = await _service.ObtenerTodasLasMateriasAsync();
            return Ok(materias);
        }

        [HttpPost("materias")]
        public async Task<IActionResult> AgregarMateria([FromBody] MateriaDto dto)
        {
            var materia = await _service.AgregarMateriaAsync(dto);
            return CreatedAtAction(nameof(ObtenerMaterias), new { id = materia.Id }, materia);
        }

        [HttpPut("materias/{materiaId}")]
        public async Task<IActionResult> ActualizarMateria(int materiaId, [FromBody] MateriaDto dto)
        {
            var materia = await _service.ActualizarMateriaAsync(materiaId, dto);
            return Ok(materia);
        }

        [HttpDelete("materias/{materiaId}")]
        public async Task<IActionResult> EliminarMateria(int materiaId)
        {
            var resultado = await _service.EliminarMateriaAsync(materiaId);
            if (!resultado)
                return NotFound("Materia no encontrada");

            return Ok(new { Mensaje = "Materia eliminada exitosamente" });
        }

        // Gestión de Inscripciones
        [HttpGet("inscripciones")]
        public async Task<IActionResult> ObtenerInscripciones()
        {
            var inscripciones = await _service.ObtenerTodasLasInscripcionesAsync();
            return Ok(inscripciones);
        }

        // Dashboard Stats
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var estudiantes = await _service.ObtenerTodosLosEstudiantesAsync();
            var profesores = await _service.ObtenerTodosLosProfesoresAsync();
            var materias = await _service.ObtenerTodasLasMateriasAsync();
            var inscripciones = await _service.ObtenerTodasLasInscripcionesAsync();

            var stats = new
            {
                TotalEstudiantes = estudiantes.Count,
                TotalProfesores = profesores.Count,
                TotalMaterias = materias.Count,
                TotalInscripciones = inscripciones.Count,
                EstudiantesInscritos = inscripciones.Select(i => i.EstudianteId).Distinct().Count(),
                MateriasConInscripciones = inscripciones.Select(i => i.MateriaId).Distinct().Count()
            };

            return Ok(stats);
        }
    }
} 