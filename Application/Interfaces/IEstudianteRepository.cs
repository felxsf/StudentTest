using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IEstudianteRepository
    {
        Task<bool> CorreoExisteAsync(string correo);
        Task<Guid> AgregarEstudianteAsync(Estudiante estudiante);
        Task<IEnumerable<string>> ObtenerNombresPorMateriaAsync(int materiaId);
        Task<List<Materia>> ObtenerMateriasPorIdsAsync(List<int> materiasIds);
        Task<List<Inscripcion>> ObtenerInscripcionesPorEstudianteAsync(Guid estudianteId);
        Task GuardarInscripcionesAsync(List<Inscripcion> inscripciones);
        Task EliminarInscripcionesEstudianteAsync(Guid estudianteId);
        Task<Estudiante?> BuscarPorCorreoAsync(string correo);
        Task<List<Estudiante>> ObtenerTodosLosEstudiantesAsync();
        Task<List<Materia>> ObtenerTodasLasMateriasAsync();
        Task<List<Profesor>> ObtenerTodosLosProfesoresAsync();
        Task<List<Inscripcion>> ObtenerTodasLasInscripcionesAsync();
        Task<Estudiante?> ObtenerEstudiantePorIdAsync(Guid estudianteId);
        Task<List<Inscripcion>> ObtenerInscripcionesPorEstudianteConDetallesAsync(Guid estudianteId);
        
        // Métodos de administración
        Task<bool> EliminarEstudianteAsync(Guid estudianteId);
        Task<bool> EliminarProfesorAsync(int profesorId);
        Task<Profesor> AgregarProfesorAsync(Profesor profesor);
        Task<Profesor?> ObtenerProfesorPorIdAsync(int profesorId);
        Task<Profesor> ActualizarProfesorAsync(Profesor profesor);
        Task<Materia> AgregarMateriaAsync(Materia materia);
        Task<Materia?> ObtenerMateriaPorIdAsync(int materiaId);
        Task<Materia> ActualizarMateriaAsync(Materia materia);
        Task<bool> EliminarMateriaAsync(int materiaId);
    }
}
