using Application.DTOs;

namespace Application.Interfaces
{
    public interface IEstudianteService
    {
        Task<Guid> RegistrarAsync(EstudianteDto dto);
        Task<IEnumerable<string>> ObtenerNombresPorMateria(int materiaId);
        Task InscribirMateriasAsync(InscripcionDto dto);
        Task ActualizarInscripcionesAsync(InscripcionDto dto);
        Task<AuthResponseDto> LoginAsync(AuthRequestDto dto);
        Task<List<EstudianteDto>> ObtenerTodosLosEstudiantesAsync();
        Task<List<MateriaDto>> ObtenerTodasLasMateriasAsync();
        Task<List<ProfesorDto>> ObtenerTodosLosProfesoresAsync();
        Task<List<InscripcionCompletaDto>> ObtenerTodasLasInscripcionesAsync();
        Task<EstudianteDto> ObtenerPerfilEstudianteAsync(Guid estudianteId);
        Task<List<InscripcionCompletaDto>> ObtenerInscripcionesEstudianteAsync(Guid estudianteId);
        Task<List<string>> ObtenerCompañerosPorMateriaAsync(int materiaId, Guid estudianteId);
        Task<Guid> RegistrarAdminAsync(AdminDto dto);
        
        // Métodos de administración
        Task<bool> EliminarEstudianteAsync(Guid estudianteId);
        Task<bool> EliminarProfesorAsync(int profesorId);
        Task<ProfesorDto> AgregarProfesorAsync(ProfesorDto dto);
        Task<ProfesorDto> ActualizarProfesorAsync(int profesorId, ProfesorDto dto);
        Task<MateriaDto> AgregarMateriaAsync(MateriaDto dto);
        Task<MateriaDto> ActualizarMateriaAsync(int materiaId, MateriaDto dto);
        Task<bool> EliminarMateriaAsync(int materiaId);
    }
}
