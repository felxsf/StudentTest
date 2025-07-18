# Documentación del Código - Sistema de Registro de Estudiantes

## Índice

1. [Introducción](#introducción)
2. [Estructura del Proyecto](#estructura-del-proyecto)
3. [Backend](#backend)
   - [Configuración de la Aplicación](#configuración-de-la-aplicación)
   - [Modelo de Datos](#modelo-de-datos)
   - [Servicios](#servicios)
   - [Controladores](#controladores)
   - [Repositorios](#repositorios)
   - [Middleware y Filtros](#middleware-y-filtros)
   - [Logging](#logging)
4. [Frontend](#frontend)
   - [Estructura de Componentes](#estructura-de-componentes)
   - [Autenticación](#autenticación)
   - [Comunicación con la API](#comunicación-con-la-api)
   - [Páginas Principales](#páginas-principales)
5. [Flujos de Trabajo Principales](#flujos-de-trabajo-principales)
   - [Registro e Inicio de Sesión](#registro-e-inicio-de-sesión)
   - [Inscripción de Materias](#inscripción-de-materias)
   - [Administración](#administración)
6. [Pruebas](#pruebas)
   - [Pruebas Unitarias](#pruebas-unitarias)
   - [Mocking](#mocking)

## Introducción

Este documento proporciona una explicación detallada del código fuente del Sistema de Registro de Estudiantes. El objetivo es facilitar la comprensión de la implementación técnica y servir como referencia para desarrolladores que trabajen en el proyecto.

## Estructura del Proyecto

El proyecto sigue una arquitectura de capas basada en Clean Architecture, con una clara separación de responsabilidades:

```
StudentTest/
├── API/                 # Capa de presentación
├── Application/         # Capa de aplicación
├── Domain/              # Capa de dominio
├── Infrastructure/      # Capa de infraestructura
├── Tests/               # Pruebas unitarias
└── frontend/            # Aplicación React
```

## Backend

### Configuración de la Aplicación

La configuración principal de la aplicación se encuentra en `Program.cs`. Este archivo es el punto de entrada de la aplicación ASP.NET Core y configura todos los servicios necesarios:

```csharp
// Configuración de Serilog para logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs" },
        columnOptions: new ColumnOptions())
    .CreateLogger();

builder.Host.UseSerilog();

// Configuración de la base de datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de servicios y repositorios
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddHttpContextAccessor();

// Configuración de autenticación JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "StudentTest",
            ValidAudience = "StudentTest",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_super_secreta_123!_muy_larga_para_jwt_256_bits_minimo_requerido"))
        };
    });

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "https://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

Esta configuración establece:

1. **Logging**: Configuración de Serilog para registrar logs en la consola y en SQL Server.
2. **Base de datos**: Configuración de Entity Framework Core con SQL Server.
3. **Inyección de dependencias**: Registro de servicios y repositorios.
4. **Autenticación**: Configuración de JWT Bearer para autenticación basada en tokens.
5. **CORS**: Configuración para permitir solicitudes desde la aplicación React.

### Modelo de Datos

#### Entidad Estudiante

```csharp
public class Estudiante
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Correo { get; set; }
    public string PasswordHash { get; set; }
    public ICollection<Inscripcion> Inscripciones { get; set; }
    public string Rol { get; set; } = "Estudiante";
}
```

La entidad `Estudiante` representa a un usuario del sistema. Puede tener el rol de "Estudiante" o "Admin". La propiedad `Inscripciones` es una colección de relaciones con materias.

#### Entidad Profesor

```csharp
public class Profesor
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public ICollection<Materia> Materias { get; set; }
}
```

La entidad `Profesor` representa a un docente que puede impartir múltiples materias.

#### Entidad Materia

```csharp
public class Materia
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Creditos { get; set; } // Siempre 3
    public int ProfesorId { get; set; }
    public Profesor Profesor { get; set; }
}
```

La entidad `Materia` representa un curso que puede ser impartido por un profesor y al que pueden inscribirse estudiantes.

#### Entidad Inscripcion

```csharp
public class Inscripcion
{
    public int Id { get; set; }
    public Guid EstudianteId { get; set; }
    public Estudiante Estudiante { get; set; }
    public int MateriaId { get; set; }
    public Materia Materia { get; set; }
}
```

La entidad `Inscripcion` representa la relación muchos a muchos entre estudiantes y materias.

### Servicios

#### EstudianteService

El servicio `EstudianteService` implementa la lógica de negocio relacionada con los estudiantes:

```csharp
public class EstudianteService : IEstudianteService
{
    private readonly IEstudianteRepository _repository;
    private readonly IConfiguration _configuration;

    public EstudianteService(IEstudianteRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<EstudianteDto> RegistrarAsync(RegistroDto dto)
    {
        // Validar que el correo no exista
        var existeCorreo = await _repository.ExisteCorreoAsync(dto.Correo);
        if (existeCorreo)
            throw new InvalidOperationException("El correo ya está registrado.");

        // Crear nuevo estudiante
        var estudiante = new Estudiante
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre,
            Correo = dto.Correo,
            PasswordHash = HashPassword(dto.Password),
            Rol = dto.EsAdmin ? "Admin" : "Estudiante"
        };

        await _repository.AgregarEstudianteAsync(estudiante);

        return new EstudianteDto
        {
            Id = estudiante.Id,
            Nombre = estudiante.Nombre,
            Correo = estudiante.Correo,
            Rol = estudiante.Rol
        };
    }

    public async Task<IEnumerable<string>> ObtenerNombresPorMateria(int materiaId)
    {
        return await _repository.ObtenerNombresPorMateriaAsync(materiaId);
    }

    public async Task InscribirMateriasAsync(InscripcionDto dto)
    {
        // Validar que sean exactamente 3 materias
        if (dto.MateriasIds.Count != 3)
            throw new InvalidOperationException("Debes seleccionar exactamente 3 materias para inscribirte.");

        // Obtener las materias seleccionadas
        var materias = await _repository.ObtenerMateriasPorIdsAsync(dto.MateriasIds);
        if (materias.Count != 3)
            throw new InvalidOperationException("Una o más materias seleccionadas no existen en el sistema.");

        // Validar que sean de diferentes profesores
        var profesores = materias.Select(m => m.ProfesorId).Distinct();
        if (profesores.Count() != 3)
            throw new InvalidOperationException("No puedes inscribirte en materias con el mismo profesor.");

        // Eliminar inscripciones existentes
        await _repository.EliminarInscripcionesAsync(dto.EstudianteId);

        // Crear nuevas inscripciones
        foreach (var materiaId in dto.MateriasIds)
        {
            await _repository.AgregarInscripcionAsync(new Inscripcion
            {
                EstudianteId = dto.EstudianteId,
                MateriaId = materiaId
            });
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var estudiante = await _repository.ObtenerPorCorreoAsync(dto.Correo);
        if (estudiante == null)
            throw new InvalidOperationException("Credenciales inválidas.");

        var passwordHash = HashPassword(dto.Password);
        if (estudiante.PasswordHash != passwordHash)
            throw new InvalidOperationException("Credenciales inválidas.");

        var token = GenerarToken(estudiante);

        return new AuthResponseDto
        {
            Token = token,
            Usuario = new EstudianteDto
            {
                Id = estudiante.Id,
                Nombre = estudiante.Nombre,
                Correo = estudiante.Correo,
                Rol = estudiante.Rol
            }
        };
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GenerarToken(Estudiante estudiante)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, estudiante.Id.ToString()),
            new Claim(ClaimTypes.Name, estudiante.Nombre),
            new Claim(ClaimTypes.Email, estudiante.Correo),
            new Claim(ClaimTypes.Role, estudiante.Rol)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_super_secreta_123!_muy_larga_para_jwt_256_bits_minimo_requerido"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "StudentTest",
            audience: "StudentTest",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

Este servicio implementa:

1. **Registro de estudiantes**: Validación de correo único y creación de cuenta.
2. **Inscripción de materias**: Aplicación de reglas de negocio (3 materias, diferentes profesores).
3. **Autenticación**: Verificación de credenciales y generación de token JWT.
4. **Consulta de compañeros**: Obtención de estudiantes por materia.

### Controladores

#### EstudiantesController

El controlador `EstudiantesController` expone los endpoints de la API relacionados con estudiantes:

```csharp
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
    public async Task<IActionResult> Registrar(RegistroDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            _loggingService.LogInformation("Iniciando registro de estudiante: {Correo}", dto.Correo);
            var resultado = await _service.RegistrarAsync(dto);
            _loggingService.LogAudit("Registro", "Anónimo", $"Registro exitoso para {dto.Correo}", true);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Error al registrar estudiante: {Correo}", dto.Correo);
            _loggingService.LogAudit("Registro", "Anónimo", $"Registro fallido para {dto.Correo}", false);
            return BadRequest(new { mensaje = ex.Message });
        }
        finally
        {
            stopwatch.Stop();
            _loggingService.LogPerformance("Registro", stopwatch.ElapsedMilliseconds, $"Registro de {dto.Correo}");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            _loggingService.LogInformation("Intento de login: {Correo}", dto.Correo);
            var resultado = await _service.LoginAsync(dto);
            _loggingService.LogAudit("Login", resultado.Usuario.Id.ToString(), $"Login exitoso para {dto.Correo}", true);
            _loggingService.LogSecurity("Login", resultado.Usuario.Id.ToString(), $"Login exitoso para {dto.Correo}", true);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Error en login: {Correo}", dto.Correo);
            _loggingService.LogAudit("Login", "Anónimo", $"Login fallido para {dto.Correo}", false);
            _loggingService.LogSecurity("Login", "Anónimo", $"Login fallido para {dto.Correo}", false);
            return BadRequest(new { mensaje = ex.Message });
        }
        finally
        {
            stopwatch.Stop();
            _loggingService.LogPerformance("Login", stopwatch.ElapsedMilliseconds, $"Login de {dto.Correo}");
        }
    }

    [Authorize(Roles = "Estudiante")]
    [HttpGet("compañeros/{materiaId}")]
    public async Task<IActionResult> ObtenerCompañeros(int materiaId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _loggingService.LogInformation("Consulta de compañeros para materia {MateriaId} por usuario {UserId}", materiaId, userId);
            
            var nombres = await _service.ObtenerNombresPorMateria(materiaId);
            return Ok(nombres);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Error al obtener compañeros para materia {MateriaId}", materiaId);
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [Authorize(Roles = "Estudiante")]
    [HttpPost("inscribir")]
    public async Task<IActionResult> InscribirMaterias(InscripcionDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Verificar que el estudiante solo pueda inscribirse a sí mismo
            var estudianteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (estudianteIdClaim == null || !Guid.TryParse(estudianteIdClaim, out var estudianteId) || estudianteId != dto.EstudianteId)
            {
                _loggingService.LogSecurity("InscribirMaterias", estudianteIdClaim, "Intento de inscribir a otro estudiante", false);
                return Forbid();
            }

            _loggingService.LogInformation("Inscripción de materias para estudiante {EstudianteId}", dto.EstudianteId);
            await _service.InscribirMateriasAsync(dto);
            _loggingService.LogAudit("InscribirMaterias", dto.EstudianteId.ToString(), $"Inscripción exitosa en {dto.MateriasIds.Count} materias", true);
            _loggingService.LogBusiness("InscribirMaterias", $"Estudiante {dto.EstudianteId} inscrito en materias {string.Join(", ", dto.MateriasIds)}");
            return Ok(new { mensaje = "Inscripción exitosa" });
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Error al inscribir materias para estudiante {EstudianteId}", dto.EstudianteId);
            _loggingService.LogAudit("InscribirMaterias", dto.EstudianteId.ToString(), "Inscripción fallida", false);
            return BadRequest(new { mensaje = ex.Message });
        }
        finally
        {
            stopwatch.Stop();
            _loggingService.LogPerformance("InscribirMaterias", stopwatch.ElapsedMilliseconds, $"Inscripción de estudiante {dto.EstudianteId}");
        }
    }
}
```

Este controlador implementa:

1. **Registro y login**: Endpoints públicos para autenticación.
2. **Consulta de compañeros**: Endpoint protegido para ver estudiantes en una materia.
3. **Inscripción de materias**: Endpoint protegido con verificación de identidad.
4. **Logging completo**: Registro de información, auditoría, seguridad y rendimiento.

### Middleware y Filtros

#### ErrorHandlingMiddleware

El middleware `ErrorHandlingMiddleware` captura excepciones no manejadas en el pipeline de ASP.NET Core:

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggingService _loggingService;

    public ErrorHandlingMiddleware(RequestDelegate next, ILoggingService loggingService)
    {
        _next = next;
        _loggingService = loggingService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _loggingService.LogError(exception, "Error no manejado en la aplicación");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            error = "Ha ocurrido un error en el servidor.",
            details = exception.Message
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

#### GlobalExceptionFilter

El filtro `GlobalExceptionFilter` captura excepciones en los controladores:

```csharp
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILoggingService _loggingService;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionFilter(ILoggingService loggingService, IWebHostEnvironment env)
    {
        _loggingService = loggingService;
        _env = env;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        _loggingService.LogError(exception, "Error en controlador: {Controller}.{Action}",
            context.RouteData.Values["controller"], context.RouteData.Values["action"]);

        var errorResponse = new ErrorResponse
        {
            Message = "Ha ocurrido un error al procesar la solicitud.",
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        // En desarrollo, incluir detalles del error
        if (_env.IsDevelopment())
        {
            errorResponse.Details = exception.Message;
            errorResponse.StackTrace = exception.StackTrace;
        }

        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = errorResponse.StatusCode
        };

        context.ExceptionHandled = true;
    }
}
```

### Logging

#### LoggingService

El servicio `LoggingService` proporciona métodos para diferentes tipos de logs:

```csharp
public class LoggingService : ILoggingService
{
    private readonly Serilog.ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingService(IHttpContextAccessor httpContextAccessor)
    {
        _logger = Serilog.Log.Logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public void LogInformation(string messageTemplate, params object[] propertyValues)
    {
        _logger.Information(messageTemplate, propertyValues);
    }

    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        _logger.Warning(messageTemplate, propertyValues);
    }

    public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        _logger.Error(exception, messageTemplate, propertyValues);
    }

    public void LogAudit(string action, string userId, string details, bool success = true)
    {
        _logger
            .ForContext("LogType", "Audit")
            .ForContext("UserId", userId)
            .ForContext("Action", action)
            .ForContext("Success", success)
            .ForContext("Details", details)
            .Information("Audit: {Action} por usuario {UserId}. Resultado: {Success}. Detalles: {Details}", 
                action, userId, success, details);
    }

    public void LogBusiness(string action, string details, object data = null)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anónimo";
        
        _logger
            .ForContext("LogType", "Business")
            .ForContext("UserId", userId)
            .ForContext("Action", action)
            .ForContext("Details", details)
            .ForContext("Data", data != null ? JsonSerializer.Serialize(data) : null)
            .Information("Business: {Action}. Detalles: {Details}", action, details);
    }

    public void LogSecurity(string action, string userId, string details, bool success = true)
    {
        _logger
            .ForContext("LogType", "Security")
            .ForContext("UserId", userId)
            .ForContext("Action", action)
            .ForContext("Success", success)
            .ForContext("Details", details)
            .Information("Security: {Action} por usuario {UserId}. Resultado: {Success}. Detalles: {Details}", 
                action, userId, success, details);
    }

    public void LogPerformance(string action, long elapsedMilliseconds, string details = null)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anónimo";
        
        _logger
            .ForContext("LogType", "Performance")
            .ForContext("UserId", userId)
            .ForContext("Action", action)
            .ForContext("ElapsedMilliseconds", elapsedMilliseconds)
            .ForContext("Details", details)
            .Information("Performance: {Action} completado en {ElapsedMilliseconds}ms. Detalles: {Details}", 
                action, elapsedMilliseconds, details);
    }
}
```

Este servicio proporciona métodos para:

1. **Logs de información, advertencia y error**: Logs básicos.
2. **Logs de auditoría**: Registro de acciones importantes de usuarios.
3. **Logs de negocio**: Registro de eventos relacionados con reglas de negocio.
4. **Logs de seguridad**: Registro de eventos de seguridad.
5. **Logs de rendimiento**: Registro de tiempos de ejecución.

## Frontend

### Estructura de Componentes

La aplicación frontend está estructurada en componentes React con TypeScript:

#### App.tsx

```tsx
function App() {
  return (
    <AuthProvider>
      <ToastContainer position="top-right" autoClose={3000} />
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Navigate to="/login" />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/dashboard" element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          } />
          <Route path="/admin" element={
            <ProtectedRoute requiredRole="Admin">
              <AdminDashboard />
            </ProtectedRoute>
          } />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
```

Este componente configura:

1. **Rutas**: Definición de rutas públicas y protegidas.
2. **Autenticación**: Proveedor de contexto de autenticación.
3. **Notificaciones**: Configuración de ToastContainer para mensajes.

### Autenticación

#### AuthContext.tsx

```tsx
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    // Verificar si hay un usuario en localStorage al cargar
    const storedUser = localStorage.getItem('user');
    const token = localStorage.getItem('token');
    
    if (storedUser && token) {
      try {
        setUser(JSON.parse(storedUser));
      } catch (error) {
        console.error('Error parsing stored user:', error);
        localStorage.removeItem('user');
        localStorage.removeItem('token');
      }
    }
  }, []);

  const login = (token: string, userData: User) => {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(userData));
    setUser(userData);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  const isAuthenticated = () => {
    return !!user && !!localStorage.getItem('token');
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, isAuthenticated }}>
      {children}
    </AuthContext.Provider>
  );
};
```

Este contexto proporciona:

1. **Estado de autenticación**: Almacenamiento del usuario actual.
2. **Funciones de login y logout**: Gestión de tokens y datos de usuario.
3. **Persistencia**: Almacenamiento en localStorage para mantener la sesión.

### Comunicación con la API

#### api.ts

```typescript
const api = axios.create({
  baseURL: 'https://localhost:7259/api',
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true
});

// Interceptor para agregar el token a las peticiones
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para manejar errores de autenticación
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Limpiar localStorage y redirigir a login
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Servicios de autenticación
export const authService = {
  login: (credentials: LoginRequest) => api.post<AuthResponse>('/estudiantes/login', credentials),
  register: (userData: RegisterRequest) => api.post<User>('/estudiantes/registro', userData),
  registerAdmin: (userData: RegisterAdminRequest) => api.post<User>('/admin/registro', userData)
};

// Servicios de estudiante
export const studentService = {
  getProfile: () => api.get<User>('/estudiantes/perfil'),
  enrollSubjects: (data: { estudianteId: string, materiasIds: number[] }) => 
    api.post('/estudiantes/inscribir', data),
  updateEnrollments: (data: { estudianteId: string, materiasIds: number[] }) => 
    api.post('/estudiantes/actualizar-inscripciones', data),
  getClassmates: (subjectId: number) => 
    api.get<string[]>(`/estudiantes/compañeros/${subjectId}`)
};

// Servicios públicos
export const publicService = {
  getAllSubjects: () => api.get<Materia[]>('/public/materias'),
  getAllProfessors: () => api.get<Profesor[]>('/public/profesores')
};

// Servicios de administrador
export const adminService = {
  getAllStudents: () => api.get<User[]>('/admin/estudiantes'),
  getAllEnrollments: () => api.get('/admin/inscripciones'),
  searchLogs: (filters: any) => api.post('/admin/logs/buscar', filters)
};
```

Este módulo proporciona:

1. **Cliente HTTP**: Configuración de Axios con URL base.
2. **Interceptores**: Manejo de tokens y errores de autenticación.
3. **Servicios**: Funciones para comunicación con diferentes endpoints de la API.

### Páginas Principales

#### Dashboard.tsx

```tsx
const Dashboard: React.FC = () => {
  const { user } = useContext(AuthContext);
  const [materias, setMaterias] = useState<Materia[]>([]);
  const [profesores, setProfesores] = useState<Profesor[]>([]);
  const [materiasInscritas, setMateriasInscritas] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [editMode, setEditMode] = useState(false);
  const [selectedMaterias, setSelectedMaterias] = useState<number[]>([]);
  const [stats, setStats] = useState({
    totalEstudiantes: 0,
    totalMaterias: 0,
    totalInscripciones: 0,
    misInscripciones: 0
  });

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        
        // Cargar materias y profesores
        const [materiasRes, profesoresRes, perfilRes] = await Promise.all([
          publicService.getAllSubjects(),
          publicService.getAllProfessors(),
          studentService.getProfile()
        ]);

        setMaterias(materiasRes.data);
        setProfesores(profesoresRes.data);
        
        // Obtener materias inscritas
        if (perfilRes.data.materiasInscritas) {
          const ids = perfilRes.data.materiasInscritas.map(m => m.id);
          setMateriasInscritas(ids);
          setSelectedMaterias(ids);
        }

        // Calcular estadísticas
        setStats({
          totalEstudiantes: 100, // Ejemplo
          totalMaterias: materiasRes.data.length,
          totalInscripciones: 300, // Ejemplo
          misInscripciones: perfilRes.data.materiasInscritas?.length || 0
        });
      } catch (error) {
        console.error('Error cargando datos:', error);
        toast.error('Error al cargar los datos');
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const handleMateriaChange = (materiaId: number) => {
    setSelectedMaterias(prev => {
      if (prev.includes(materiaId)) {
        return prev.filter(id => id !== materiaId);
      } else {
        if (prev.length >= 3) {
          toast.warning('Solo puedes seleccionar 3 materias');
          return prev;
        }
        return [...prev, materiaId];
      }
    });
  };

  const handleSaveInscripciones = async () => {
    if (selectedMaterias.length !== 3) {
      toast.error('Debes seleccionar exactamente 3 materias');
      return;
    }

    // Verificar que sean de diferentes profesores
    const profesoresIds = selectedMaterias.map(materiaId => {
      const materia = materias.find(m => m.id === materiaId);
      return materia?.profesorId;
    });

    const uniqueProfesores = new Set(profesoresIds);
    if (uniqueProfesores.size !== 3) {
      toast.error('Debes seleccionar materias de diferentes profesores');
      return;
    }

    try {
      await studentService.updateEnrollments({
        estudianteId: user?.id || '',
        materiasIds: selectedMaterias
      });

      setMateriasInscritas(selectedMaterias);
      setEditMode(false);
      toast.success('Inscripciones actualizadas correctamente');
    } catch (error: any) {
      console.error('Error actualizando inscripciones:', error);
      toast.error(error.response?.data?.mensaje || 'Error al actualizar inscripciones');
    }
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <Navigation />
      
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-6">Dashboard de Estudiante</h1>
        
        {/* Estadísticas */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          <StatCard title="Total Estudiantes" value={stats.totalEstudiantes} icon={<UserGroupIcon className="h-8 w-8 text-blue-500" />} />
          <StatCard title="Total Materias" value={stats.totalMaterias} icon={<BookOpenIcon className="h-8 w-8 text-green-500" />} />
          <StatCard title="Total Inscripciones" value={stats.totalInscripciones} icon={<ClipboardListIcon className="h-8 w-8 text-purple-500" />} />
          <StatCard title="Mis Inscripciones" value={stats.misInscripciones} icon={<AcademicCapIcon className="h-8 w-8 text-yellow-500" />} />
        </div>
        
        {/* Materias Inscritas */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-semibold">Mis Materias Inscritas</h2>
            <button 
              onClick={() => setEditMode(!editMode)}
              className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition"
            >
              {editMode ? 'Cancelar' : 'Editar Inscripciones'}
            </button>
          </div>
          
          {loading ? (
            <p>Cargando materias...</p>
          ) : editMode ? (
            <div>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                {materias.map(materia => {
                  const profesor = profesores.find(p => p.id === materia.profesorId);
                  return (
                    <div key={materia.id} className="border rounded p-4">
                      <div className="flex items-start">
                        <input
                          type="checkbox"
                          checked={selectedMaterias.includes(materia.id)}
                          onChange={() => handleMateriaChange(materia.id)}
                          className="mt-1 mr-3"
                        />
                        <div>
                          <h3 className="font-medium">{materia.nombre}</h3>
                          <p className="text-sm text-gray-600">Profesor: {profesor?.nombre}</p>
                          <p className="text-sm text-gray-600">Créditos: {materia.creditos}</p>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
              <div className="flex justify-end">
                <button
                  onClick={handleSaveInscripciones}
                  className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition"
                >
                  Guardar Cambios
                </button>
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {materiasInscritas.length > 0 ? (
                materias
                  .filter(materia => materiasInscritas.includes(materia.id))
                  .map(materia => {
                    const profesor = profesores.find(p => p.id === materia.profesorId);
                    return (
                      <div key={materia.id} className="border rounded p-4">
                        <h3 className="font-medium">{materia.nombre}</h3>
                        <p className="text-sm text-gray-600">Profesor: {profesor?.nombre}</p>
                        <p className="text-sm text-gray-600">Créditos: {materia.creditos}</p>
                      </div>
                    );
                  })
              ) : (
                <p className="col-span-3">No tienes materias inscritas. Haz clic en "Editar Inscripciones" para comenzar.</p>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
```

Este componente implementa:

1. **Carga de datos**: Obtención de materias, profesores y perfil del estudiante.
2. **Gestión de inscripciones**: Visualización y edición de materias inscritas.
3. **Validación de reglas**: Verificación de 3 materias y diferentes profesores.
4. **Estadísticas**: Visualización de métricas relevantes.

## Flujos de Trabajo Principales

### Registro e Inicio de Sesión

1. **Registro**:
   - El usuario ingresa nombre, correo y contraseña.
   - El sistema valida que el correo no exista.
   - Se crea un nuevo usuario con rol "Estudiante".
   - Se almacena la contraseña con hash SHA-256.

2. **Inicio de Sesión**:
   - El usuario ingresa correo y contraseña.
   - El sistema valida las credenciales.
   - Se genera un token JWT con claims de identidad y rol.
   - El token se almacena en localStorage.

### Inscripción de Materias

1. **Visualización de Materias**:
   - El estudiante ve la lista de todas las materias disponibles.
   - Se muestra información de cada materia (nombre, profesor, créditos).

2. **Selección de Materias**:
   - El estudiante selecciona materias para inscribirse.
   - El sistema valida que sean exactamente 3 materias.
   - El sistema valida que sean de diferentes profesores.

3. **Confirmación**:
   - Se eliminan inscripciones anteriores.
   - Se crean nuevas inscripciones.
   - Se registra la acción en logs de auditoría y negocio.

### Administración

1. **Panel de Administración**:
   - Solo accesible para usuarios con rol "Admin".
   - Muestra estadísticas generales del sistema.

2. **Gestión de Estudiantes**:
   - Visualización de todos los estudiantes.
   - Posibilidad de registrar nuevos administradores.

3. **Consulta de Logs**:
   - Filtrado de logs por tipo, fecha, usuario, etc.
   - Visualización de métricas de rendimiento y seguridad.

## Pruebas

### Pruebas Unitarias

#### EstudiantesControllerTests

```csharp
public class EstudiantesControllerTests
{
    private readonly Mock<IEstudianteService> _mockService;
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly EstudiantesController _controller;

    public EstudiantesControllerTests()
    {
        _mockService = new Mock<IEstudianteService>();
        _mockLoggingService = new Mock<ILoggingService>();
        _controller = new EstudiantesController(_mockService.Object, _mockLoggingService.Object);
    }

    [Fact]
    public async Task ObtenerTodosLosEstudiantes_CuandoHayEstudiantes_DebeRetornarOkConLista()
    {
        // Arrange
        var estudiantes = new List<EstudianteDto>
        {
            new EstudianteDto { Id = Guid.NewGuid(), Nombre = "Estudiante 1", Correo = "est1@test.com" },
            new EstudianteDto { Id = Guid.NewGuid(), Nombre = "Estudiante 2", Correo = "est2@test.com" }
        };

        _mockService.Setup(s => s.ObtenerTodosLosEstudiantesAsync()).ReturnsAsync(estudiantes);

        // Act
        var result = await _controller.ObtenerTodosLosEstudiantes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEstudiantes = Assert.IsType<List<EstudianteDto>>(okResult.Value);
        Assert.Equal(2, returnedEstudiantes.Count);
        _mockLoggingService.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodosLosEstudiantes_CuandoNoHayEstudiantes_DebeRetornarOkConListaVacia()
    {
        // Arrange
        var estudiantes = new List<EstudianteDto>();
        _mockService.Setup(s => s.ObtenerTodosLosEstudiantesAsync()).ReturnsAsync(estudiantes);

        // Act
        var result = await _controller.ObtenerTodosLosEstudiantes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEstudiantes = Assert.IsType<List<EstudianteDto>>(okResult.Value);
        Assert.Empty(returnedEstudiantes);
        _mockLoggingService.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
}
```

### Mocking

#### EstudianteServiceTests

```csharp
public class EstudianteServiceTests
{
    private readonly Mock<IEstudianteRepository> _mockRepository;
    private readonly IEstudianteService _service;

    public EstudianteServiceTests()
    {
        _mockRepository = new Mock<IEstudianteRepository>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c[It.IsAny<string>()]).Returns("clave_super_secreta_123");
        _service = new EstudianteService(_mockRepository.Object, mockConfiguration.Object);
    }

    [Fact]
    public async Task ObtenerTodosLosEstudiantesAsync_CuandoHayEstudiantes_DebeRetornarListaDeEstudiantes()
    {
        // Arrange
        var estudiantes = new List<Estudiante>
        {
            new Estudiante { Id = Guid.NewGuid(), Nombre = "Estudiante 1", Correo = "est1@test.com" },
            new Estudiante { Id = Guid.NewGuid(), Nombre = "Estudiante 2", Correo = "est2@test.com" }
        };

        _mockRepository.Setup(r => r.ObtenerTodosLosEstudiantesAsync()).ReturnsAsync(estudiantes);

        // Act
        var result = await _service.ObtenerTodosLosEstudiantesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.ObtenerTodosLosEstudiantesAsync(), Times.Once);
    }

    [Fact]
    public async Task ObtenerTodosLosEstudiantesAsync_CuandoNoHayEstudiantes_DebeRetornarListaVacia()
    {
        // Arrange
        var estudiantes = new List<Estudiante>();
        _mockRepository.Setup(r => r.ObtenerTodosLosEstudiantesAsync()).ReturnsAsync(estudiantes);

        // Act
        var result = await _service.ObtenerTodosLosEstudiantesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(r => r.ObtenerTodosLosEstudiantesAsync(), Times.Once);
    }
}
```

Estas pruebas demuestran:

1. **Aislamiento**: Uso de mocks para aislar la unidad bajo prueba.
2. **Verificación de comportamiento**: Comprobación de llamadas a dependencias.
3. **Verificación de resultados**: Validación de valores retornados.
4. **Cobertura de casos**: Pruebas para diferentes escenarios (con y sin datos).