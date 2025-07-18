using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace Application.Services
{
    public class EstudianteService : IEstudianteService
    {
        private readonly IEstudianteRepository _repository;

        public EstudianteService(IEstudianteRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> RegistrarAsync(EstudianteDto dto)
        {
            if (await _repository.CorreoExisteAsync(dto.Correo))
                throw new InvalidOperationException("El correo electrónico ya está registrado en el sistema. Por favor, utiliza un correo diferente.");

            var estudiante = new Estudiante
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                PasswordHash = HashPassword(dto.Password),
                Rol = "Estudiante"
            };

            return await _repository.AgregarEstudianteAsync(estudiante);
        }

        public async Task<IEnumerable<string>> ObtenerNombresPorMateria(int materiaId)
        {
            return await _repository.ObtenerNombresPorMateriaAsync(materiaId);
        }

        public async Task InscribirMateriasAsync(InscripcionDto dto)
        {
            if (dto.MateriasIds.Count != 3)
                throw new InvalidOperationException("Debes seleccionar exactamente 3 materias para inscribirte.");

            var materias = await _repository.ObtenerMateriasPorIdsAsync(dto.MateriasIds);
            if (materias.Count != 3)
                throw new InvalidOperationException("Una o más materias seleccionadas no existen en el sistema. Por favor, verifica tu selección.");

            var profesores = materias.Select(m => m.ProfesorId).Distinct();
            if (profesores.Count() != 3)
                throw new InvalidOperationException("No puedes inscribirte en materias con el mismo profesor. Debes seleccionar materias de 3 profesores diferentes.");

            var inscripcionesActuales = await _repository.ObtenerInscripcionesPorEstudianteAsync(dto.EstudianteId);
            if (inscripcionesActuales.Any())
                throw new InvalidOperationException("Ya tienes inscripciones activas. Si deseas cambiar tus materias, utiliza la opción de editar inscripciones.");

            var inscripciones = materias.Select(m => new Inscripcion
            {
                EstudianteId = dto.EstudianteId,
                MateriaId = m.Id
            }).ToList();

            await _repository.GuardarInscripcionesAsync(inscripciones);
        }

        public async Task ActualizarInscripcionesAsync(InscripcionDto dto)
        {
            if (dto.MateriasIds.Count != 3)
                throw new InvalidOperationException("Debes seleccionar exactamente 3 materias para actualizar tus inscripciones.");

            var materias = await _repository.ObtenerMateriasPorIdsAsync(dto.MateriasIds);
            if (materias.Count != 3)
                throw new InvalidOperationException("Una o más materias seleccionadas no existen en el sistema. Por favor, verifica tu selección.");

            var profesores = materias.Select(m => m.ProfesorId).Distinct();
            if (profesores.Count() != 3)
                throw new InvalidOperationException("No puedes inscribirte en materias con el mismo profesor. Debes seleccionar materias de 3 profesores diferentes.");

            // Eliminar inscripciones actuales
            await _repository.EliminarInscripcionesEstudianteAsync(dto.EstudianteId);

            // Crear nuevas inscripciones
            var inscripciones = materias.Select(m => new Inscripcion
            {
                EstudianteId = dto.EstudianteId,
                MateriaId = m.Id
            }).ToList();

            await _repository.GuardarInscripcionesAsync(inscripciones);
        }

        public async Task<AuthResponseDto> LoginAsync(AuthRequestDto dto)
        {
            var estudiante = await _repository.BuscarPorCorreoAsync(dto.Correo);
            if (estudiante == null)
                throw new UnauthorizedAccessException("El correo electrónico o la contraseña son incorrectos.");

            var hash = HashPassword(dto.Password);
            if (estudiante.PasswordHash != hash)
                throw new UnauthorizedAccessException("El correo electrónico o la contraseña son incorrectos.");

            var token = GenerarToken(estudiante);

            return new AuthResponseDto
            {
                Token = token,
                Nombre = estudiante.Nombre,
                Correo = estudiante.Correo,
                Rol = estudiante.Rol
            };
        }

        private string GenerarToken(Estudiante estudiante)
        {
            var claims = new[]
   {
        new Claim(ClaimTypes.NameIdentifier, estudiante.Id.ToString()),
        new Claim(ClaimTypes.Name, estudiante.Nombre),
        new Claim(ClaimTypes.Email, estudiante.Correo),
        new Claim(ClaimTypes.Role, estudiante.Rol ?? "Estudiante"), // Aquí está el rol
        // Puedes agregar más claims personalizados si deseas:
        // new Claim("Carrera", "Ingeniería"),
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_super_secreta_123!_muy_larga_para_jwt_256_bits_minimo_requerido"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "StudentTest",
                audience: "StudentTest",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<List<EstudianteDto>> ObtenerTodosLosEstudiantesAsync()
        {
            var estudiantes = await _repository.ObtenerTodosLosEstudiantesAsync();
            return estudiantes.Select(e => new EstudianteDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Correo = e.Correo,
                Password = "", // No devolver la contraseña
                Rol = e.Rol ?? "Estudiante",
                MateriasInscritas = e.Inscripciones.Select(i => i.Materia.Nombre).ToList()
            }).ToList();
        }

        public async Task<List<MateriaDto>> ObtenerTodasLasMateriasAsync()
        {
            var materias = await _repository.ObtenerTodasLasMateriasAsync();
            return materias.Select(m => new MateriaDto
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Creditos = m.Creditos,
                ProfesorId = m.ProfesorId,
                ProfesorNombre = m.Profesor.Nombre
            }).ToList();
        }

        public async Task<List<ProfesorDto>> ObtenerTodosLosProfesoresAsync()
        {
            var profesores = await _repository.ObtenerTodosLosProfesoresAsync();
            return profesores.Select(p => new ProfesorDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Materias = p.Materias.Select(m => m.Nombre).ToList()
            }).ToList();
        }

        public async Task<List<InscripcionCompletaDto>> ObtenerTodasLasInscripcionesAsync()
        {
            var inscripciones = await _repository.ObtenerTodasLasInscripcionesAsync();
            return inscripciones.Select(i => new InscripcionCompletaDto
            {
                Id = i.Id,
                EstudianteId = i.EstudianteId,
                EstudianteNombre = i.Estudiante.Nombre,
                EstudianteCorreo = i.Estudiante.Correo,
                MateriaId = i.MateriaId,
                MateriaNombre = i.Materia.Nombre,
                MateriaCreditos = i.Materia.Creditos,
                ProfesorId = i.Materia.ProfesorId,
                ProfesorNombre = i.Materia.Profesor.Nombre
            }).ToList();
        }

        public async Task<EstudianteDto> ObtenerPerfilEstudianteAsync(Guid estudianteId)
        {
            var estudiante = await _repository.ObtenerEstudiantePorIdAsync(estudianteId);
            if (estudiante == null)
                throw new InvalidOperationException("No se encontró el perfil del estudiante solicitado.");

            return new EstudianteDto
            {
                Id = estudiante.Id,
                Nombre = estudiante.Nombre,
                Correo = estudiante.Correo,
                Password = "", // No devolver la contraseña
                Rol = estudiante.Rol ?? "Estudiante",
                MateriasInscritas = estudiante.Inscripciones.Select(i => i.Materia.Nombre).ToList()
            };
        }

        public async Task<List<InscripcionCompletaDto>> ObtenerInscripcionesEstudianteAsync(Guid estudianteId)
        {
            var inscripciones = await _repository.ObtenerInscripcionesPorEstudianteConDetallesAsync(estudianteId);
            return inscripciones.Select(i => new InscripcionCompletaDto
            {
                Id = i.Id,
                EstudianteId = i.EstudianteId,
                EstudianteNombre = i.Estudiante.Nombre,
                EstudianteCorreo = i.Estudiante.Correo,
                MateriaId = i.MateriaId,
                MateriaNombre = i.Materia.Nombre,
                MateriaCreditos = i.Materia.Creditos,
                ProfesorId = i.Materia.ProfesorId,
                ProfesorNombre = i.Materia.Profesor.Nombre
            }).ToList();
        }

        public async Task<List<string>> ObtenerCompañerosPorMateriaAsync(int materiaId, Guid estudianteId)
        {
            var nombres = await _repository.ObtenerNombresPorMateriaAsync(materiaId);
            // Excluir al estudiante actual de la lista de compañeros
            var estudiante = await _repository.ObtenerEstudiantePorIdAsync(estudianteId);
            if (estudiante != null)
            {
                return nombres.Where(n => n != estudiante.Nombre).ToList();
            }
            return nombres.ToList();
        }

        public async Task<Guid> RegistrarAdminAsync(AdminDto dto)
        {
            // Verificar código secreto de administrador
            if (dto.CodigoAdmin != "ADMIN2024")
                throw new UnauthorizedAccessException("El código de administrador proporcionado es incorrecto. Por favor, verifica el código.");

            if (await _repository.CorreoExisteAsync(dto.Correo))
                throw new InvalidOperationException("El correo electrónico ya está registrado en el sistema. Por favor, utiliza un correo diferente.");

            var admin = new Estudiante
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                PasswordHash = HashPassword(dto.Password),
                Rol = "Admin"
            };

            return await _repository.AgregarEstudianteAsync(admin);
        }

        // Métodos de administración
        public async Task<bool> EliminarEstudianteAsync(Guid estudianteId)
        {
            return await _repository.EliminarEstudianteAsync(estudianteId);
        }

        public async Task<bool> EliminarProfesorAsync(int profesorId)
        {
            return await _repository.EliminarProfesorAsync(profesorId);
        }

        public async Task<ProfesorDto> AgregarProfesorAsync(ProfesorDto dto)
        {
            var profesor = new Profesor
            {
                Nombre = dto.Nombre
            };

            var profesorCreado = await _repository.AgregarProfesorAsync(profesor);
            return new ProfesorDto
            {
                Id = profesorCreado.Id,
                Nombre = profesorCreado.Nombre,
                Materias = new List<string>()
            };
        }

        public async Task<ProfesorDto> ActualizarProfesorAsync(int profesorId, ProfesorDto dto)
        {
            var profesor = await _repository.ObtenerProfesorPorIdAsync(profesorId);
            if (profesor == null)
                throw new InvalidOperationException("El profesor no fue encontrado.");

            profesor.Nombre = dto.Nombre;
            var profesorActualizado = await _repository.ActualizarProfesorAsync(profesor);
            
            return new ProfesorDto
            {
                Id = profesorActualizado.Id,
                Nombre = profesorActualizado.Nombre,
                Materias = profesorActualizado.Materias.Select(m => m.Nombre).ToList()
            };
        }

        public async Task<MateriaDto> AgregarMateriaAsync(MateriaDto dto)
        {
            var materia = new Materia
            {
                Nombre = dto.Nombre,
                Creditos = dto.Creditos,
                ProfesorId = dto.ProfesorId
            };

            var materiaCreada = await _repository.AgregarMateriaAsync(materia);
            var materiaConProfesor = await _repository.ObtenerMateriaPorIdAsync(materiaCreada.Id);
            
            return new MateriaDto
            {
                Id = materiaCreada.Id,
                Nombre = materiaCreada.Nombre,
                Creditos = materiaCreada.Creditos,
                ProfesorId = materiaCreada.ProfesorId,
                ProfesorNombre = materiaConProfesor?.Profesor.Nombre ?? ""
            };
        }

        public async Task<MateriaDto> ActualizarMateriaAsync(int materiaId, MateriaDto dto)
        {
            var materia = await _repository.ObtenerMateriaPorIdAsync(materiaId);
            if (materia == null)
                throw new InvalidOperationException("La materia no fue encontrada.");

            materia.Nombre = dto.Nombre;
            materia.Creditos = dto.Creditos;
            materia.ProfesorId = dto.ProfesorId;

            var materiaActualizada = await _repository.ActualizarMateriaAsync(materia);
            var materiaConProfesor = await _repository.ObtenerMateriaPorIdAsync(materiaActualizada.Id);
            
            return new MateriaDto
            {
                Id = materiaActualizada.Id,
                Nombre = materiaActualizada.Nombre,
                Creditos = materiaActualizada.Creditos,
                ProfesorId = materiaActualizada.ProfesorId,
                ProfesorNombre = materiaConProfesor?.Profesor.Nombre ?? ""
            };
        }

        public async Task<bool> EliminarMateriaAsync(int materiaId)
        {
            return await _repository.EliminarMateriaAsync(materiaId);
        }
    }
}
