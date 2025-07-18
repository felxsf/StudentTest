using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class EstudianteRepository(AppDbContext context) : IEstudianteRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<bool> CorreoExisteAsync(string correo)
        {
            return await _context.Estudiantes.AnyAsync(e => e.Correo == correo);
        }

        public async Task<Guid> AgregarEstudianteAsync(Estudiante estudiante)
        {
            _context.Estudiantes.Add(estudiante);
            await _context.SaveChangesAsync();
            return estudiante.Id;
        }

        public async Task<IEnumerable<string>> ObtenerNombresPorMateriaAsync(int materiaId)
        {
            return await _context.Inscripciones
                .Where(i => i.MateriaId == materiaId)
                .Include(i => i.Estudiante)
                .Select(i => i.Estudiante.Nombre)
                .ToListAsync();
        }

        public async Task<List<Materia>> ObtenerMateriasPorIdsAsync(List<int> materiasIds)
        {
            return await _context.Materias
                .Include(m => m.Profesor)
                .Where(m => materiasIds.Contains(m.Id))
                .ToListAsync();
        }

        public async Task<List<Inscripcion>> ObtenerInscripcionesPorEstudianteAsync(Guid estudianteId)
        {
            return await _context.Inscripciones
                .Where(i => i.EstudianteId == estudianteId)
                .ToListAsync();
        }

        public async Task GuardarInscripcionesAsync(List<Inscripcion> inscripciones)
        {
            _context.Inscripciones.AddRange(inscripciones);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarInscripcionesEstudianteAsync(Guid estudianteId)
        {
            var inscripciones = await _context.Inscripciones
                .Where(i => i.EstudianteId == estudianteId)
                .ToListAsync();
            
            _context.Inscripciones.RemoveRange(inscripciones);
            await _context.SaveChangesAsync();
        }

        public async Task<Estudiante?> BuscarPorCorreoAsync(string correo)
        {
            return await _context.Estudiantes.FirstOrDefaultAsync(e => e.Correo == correo);
        }

        public async Task<List<Estudiante>> ObtenerTodosLosEstudiantesAsync()
        {
            return await _context.Estudiantes
                .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Materia)
                .ThenInclude(m => m.Profesor)
                .ToListAsync();
        }

        public async Task<List<Materia>> ObtenerTodasLasMateriasAsync()
        {
            return await _context.Materias
                .Include(m => m.Profesor)
                .ToListAsync();
        }

        public async Task<List<Profesor>> ObtenerTodosLosProfesoresAsync()
        {
            return await _context.Profesores
                .Include(p => p.Materias)
                .ToListAsync();
        }

        public async Task<List<Inscripcion>> ObtenerTodasLasInscripcionesAsync()
        {
            return await _context.Inscripciones
                .Include(i => i.Estudiante)
                .Include(i => i.Materia)
                .ThenInclude(m => m.Profesor)
                .ToListAsync();
        }

        public async Task<Estudiante?> ObtenerEstudiantePorIdAsync(Guid estudianteId)
        {
            return await _context.Estudiantes
                .Include(e => e.Inscripciones)
                .ThenInclude(i => i.Materia)
                .ThenInclude(m => m.Profesor)
                .FirstOrDefaultAsync(e => e.Id == estudianteId);
        }

        public async Task<List<Inscripcion>> ObtenerInscripcionesPorEstudianteConDetallesAsync(Guid estudianteId)
        {
            return await _context.Inscripciones
                .Include(i => i.Estudiante)
                .Include(i => i.Materia)
                .ThenInclude(m => m.Profesor)
                .Where(i => i.EstudianteId == estudianteId)
                .ToListAsync();
        }

        // Métodos de administración
        public async Task<bool> EliminarEstudianteAsync(Guid estudianteId)
        {
            var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
            if (estudiante == null) return false;

            // Eliminar inscripciones del estudiante
            var inscripciones = await _context.Inscripciones
                .Where(i => i.EstudianteId == estudianteId)
                .ToListAsync();
            
            _context.Inscripciones.RemoveRange(inscripciones);
            _context.Estudiantes.Remove(estudiante);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarProfesorAsync(int profesorId)
        {
            var profesor = await _context.Profesores.FindAsync(profesorId);
            if (profesor == null) return false;

            // Verificar si tiene materias asignadas
            var materias = await _context.Materias
                .Where(m => m.ProfesorId == profesorId)
                .ToListAsync();

            if (materias.Any())
            {
                // Eliminar inscripciones a esas materias
                var inscripciones = await _context.Inscripciones
                    .Where(i => materias.Select(m => m.Id).Contains(i.MateriaId))
                    .ToListAsync();
                
                _context.Inscripciones.RemoveRange(inscripciones);
                _context.Materias.RemoveRange(materias);
            }

            _context.Profesores.Remove(profesor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Profesor> AgregarProfesorAsync(Profesor profesor)
        {
            _context.Profesores.Add(profesor);
            await _context.SaveChangesAsync();
            return profesor;
        }

        public async Task<Profesor?> ObtenerProfesorPorIdAsync(int profesorId)
        {
            return await _context.Profesores
                .Include(p => p.Materias)
                .FirstOrDefaultAsync(p => p.Id == profesorId);
        }

        public async Task<Profesor> ActualizarProfesorAsync(Profesor profesor)
        {
            _context.Profesores.Update(profesor);
            await _context.SaveChangesAsync();
            return profesor;
        }

        public async Task<Materia> AgregarMateriaAsync(Materia materia)
        {
            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();
            return materia;
        }

        public async Task<Materia?> ObtenerMateriaPorIdAsync(int materiaId)
        {
            return await _context.Materias
                .Include(m => m.Profesor)
                .FirstOrDefaultAsync(m => m.Id == materiaId);
        }

        public async Task<Materia> ActualizarMateriaAsync(Materia materia)
        {
            _context.Materias.Update(materia);
            await _context.SaveChangesAsync();
            return materia;
        }

        public async Task<bool> EliminarMateriaAsync(int materiaId)
        {
            var materia = await _context.Materias.FindAsync(materiaId);
            if (materia == null) return false;

            // Eliminar inscripciones a esta materia
            var inscripciones = await _context.Inscripciones
                .Where(i => i.MateriaId == materiaId)
                .ToListAsync();
            
            _context.Inscripciones.RemoveRange(inscripciones);
            _context.Materias.Remove(materia);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
