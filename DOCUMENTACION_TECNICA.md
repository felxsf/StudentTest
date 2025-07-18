# Documentación Técnica - Sistema de Registro de Estudiantes

## 1. Arquitectura del Sistema

El sistema está desarrollado siguiendo una arquitectura de capas con Clean Architecture, lo que permite una clara separación de responsabilidades y facilita el mantenimiento y la escalabilidad.

### 1.1 Estructura de Capas

```
StudentTest/
├── API/                 # Capa de presentación (Controllers, Middleware, Filters)
├── Application/         # Capa de aplicación (Services, DTOs, Interfaces)
├── Domain/              # Capa de dominio (Entities)
└── Infrastructure/      # Capa de infraestructura (DbContext, Repositories, Migrations)
```

### 1.2 Descripción de Capas

#### 1.2.1 Capa de Dominio (Domain)

Contiene las entidades del negocio y la lógica de dominio central:

- **Entidades**: `Estudiante`, `Profesor`, `Materia`, `Inscripcion`, `Log`
- No tiene dependencias con otras capas
- Define las reglas de negocio fundamentales

#### 1.2.2 Capa de Aplicación (Application)

Contiene la lógica de aplicación y casos de uso:

- **DTOs**: Objetos de transferencia de datos para comunicación entre capas
- **Interfaces**: Contratos para repositorios y servicios
- **Servicios**: Implementación de la lógica de negocio
- Solo depende de la capa de Dominio

#### 1.2.3 Capa de Infraestructura (Infrastructure)

Implementa el acceso a datos y servicios externos:

- **Context**: Configuración de Entity Framework Core
- **Repositories**: Implementación de los repositorios
- **Migrations**: Migraciones de la base de datos
- **DataSeeder**: Inicialización de datos

#### 1.2.4 Capa de API (API)

Expone los endpoints REST y maneja las solicitudes HTTP:

- **Controllers**: Controladores REST
- **Middleware**: Componentes de procesamiento de solicitudes
- **Filters**: Filtros para manejo de excepciones
- **Services**: Servicios específicos de la API (logging)

## 2. Tecnologías Utilizadas

### 2.1 Backend

- **ASP.NET Core 6**: Framework principal
- **Entity Framework Core**: ORM para acceso a datos
- **SQL Server**: Base de datos relacional
- **JWT**: Autenticación basada en tokens
- **Serilog**: Logging estructurado
- **xUnit**: Framework de pruebas unitarias
- **Moq**: Framework de mocking para pruebas
- **FluentAssertions**: Aserciones expresivas para pruebas

### 2.2 Frontend

- **React 18**: Framework principal
- **TypeScript**: Tipado estático
- **React Router**: Navegación SPA
- **Axios**: Cliente HTTP
- **React Toastify**: Notificaciones
- **Tailwind CSS**: Framework de estilos
- **Headless UI**: Componentes accesibles

## 3. Modelo de Datos

### 3.1 Diagrama de Entidades

```
+-------------+       +-------------+       +-------------+
|  Estudiante |       | Inscripcion |       |   Materia   |
+-------------+       +-------------+       +-------------+
| Id (PK)     |<----->| Id (PK)     |       | Id (PK)     |
| Nombre      |       | EstudianteId|<----->| Nombre      |
| Correo      |       | MateriaId   |       | Creditos    |
| PasswordHash|       +-------------+       | ProfesorId  |
| Rol         |                             +-------------+
+-------------+                                    |
                                                   |
                                                   v
                                            +-------------+
                                            |   Profesor  |
                                            +-------------+
                                            | Id (PK)     |
                                            | Nombre      |
                                            +-------------+
```

### 3.2 Descripción de Entidades

#### 3.2.1 Estudiante

- **Id**: Identificador único (GUID)
- **Nombre**: Nombre completo del estudiante
- **Correo**: Email único del estudiante (usado para login)
- **PasswordHash**: Hash de la contraseña
- **Rol**: Rol del usuario ("Estudiante" o "Admin")
- **Inscripciones**: Colección de inscripciones del estudiante

#### 3.2.2 Profesor

- **Id**: Identificador único (int)
- **Nombre**: Nombre completo del profesor
- **Materias**: Colección de materias que imparte

#### 3.2.3 Materia

- **Id**: Identificador único (int)
- **Nombre**: Nombre de la materia
- **Creditos**: Número de créditos (default: 3)
- **ProfesorId**: ID del profesor que imparte la materia
- **Profesor**: Referencia al profesor

#### 3.2.4 Inscripcion

- **Id**: Identificador único (int)
- **EstudianteId**: ID del estudiante
- **MateriaId**: ID de la materia
- **Estudiante**: Referencia al estudiante
- **Materia**: Referencia a la materia

#### 3.2.5 Log

- **Id**: Identificador único (int)
- **Timestamp**: Fecha y hora del evento
- **Level**: Nivel de log (Information, Warning, Error, etc.)
- **Message**: Mensaje del log
- **Exception**: Detalles de la excepción (si aplica)
- **Properties**: Propiedades adicionales en formato JSON
- **LogType**: Tipo de log (Audit, Business, Performance, Security)
- **UserId**: ID del usuario que generó el evento
- **Action**: Acción realizada
- **Result**: Resultado de la acción

## 4. Seguridad

### 4.1 Autenticación

- **JWT (JSON Web Tokens)**: Implementación de autenticación basada en tokens
- **Hashing de Contraseñas**: Uso de SHA-256 para almacenar contraseñas de forma segura
- **Validación de Tokens**: Verificación de firma, emisor, audiencia y tiempo de expiración

### 4.2 Autorización

- **Roles**: Sistema de roles ("Estudiante", "Admin")
- **Atributos de Autorización**: Uso de `[Authorize(Roles = "...")]` para proteger endpoints
- **Verificación de Propiedad**: Validación de que un estudiante solo puede modificar sus propios datos

### 4.3 Middleware de Seguridad

- **CORS**: Configuración de políticas de origen cruzado
- **ErrorHandlingMiddleware**: Captura y manejo centralizado de excepciones
- **AuthorizationMiddleware**: Validación de tokens y asignación de identidad

## 5. Logging y Monitoreo

### 5.1 Sistema de Logging

- **Serilog**: Implementación de logging estructurado
- **Múltiples Destinos**: Console y SQL Server
- **Niveles de Log**: Information, Warning, Error, Critical, Debug

### 5.2 Tipos de Logs

- **Logs de Auditoría**: Registro de acciones importantes (login, registro, etc.)
- **Logs de Negocio**: Eventos relacionados con reglas de negocio
- **Logs de Rendimiento**: Medición de tiempos de respuesta
- **Logs de Seguridad**: Eventos relacionados con seguridad

### 5.3 Dashboard de Logs

- Interfaz web para visualizar y filtrar logs
- Estadísticas de rendimiento y errores
- Alertas para eventos críticos

## 6. Manejo de Errores

### 6.1 Estrategia de Manejo de Excepciones

- **GlobalExceptionFilter**: Filtro para capturar excepciones en controladores
- **ErrorHandlingMiddleware**: Middleware para capturar excepciones no manejadas
- **CustomExceptions**: Excepciones personalizadas para diferentes casos de error

### 6.2 Respuestas de Error

- Formato estandarizado de respuestas de error
- Inclusión de detalles técnicos solo en entorno de desarrollo
- Códigos HTTP apropiados según el tipo de error

## 7. Pruebas

### 7.1 Pruebas Unitarias

- **xUnit**: Framework de pruebas
- **Moq**: Mocking de dependencias
- **FluentAssertions**: Aserciones expresivas

### 7.2 Cobertura de Pruebas

- Pruebas de controladores (API)
- Pruebas de servicios (Application)
- Pruebas de entidades (Domain)

## 8. Frontend

### 8.1 Estructura de la Aplicación React

```
frontend/
├── public/             # Archivos estáticos
├── src/                # Código fuente
│   ├── components/     # Componentes reutilizables
│   ├── pages/          # Páginas principales
│   ├── services/       # Servicios de API
│   ├── types/          # Definiciones de TypeScript
│   ├── utils/          # Utilidades y contextos
│   ├── App.tsx         # Componente principal
│   └── index.tsx       # Punto de entrada
└── package.json        # Dependencias y scripts
```

### 8.2 Componentes Principales

#### 8.2.1 Layout

- **Navigation**: Barra de navegación responsive
- **ProtectedRoute**: Protección de rutas
- **AuthProvider**: Contexto de autenticación

#### 8.2.2 Páginas

- **Login**: Formulario de autenticación
- **Register**: Formulario de registro
- **Dashboard**: Panel principal del estudiante
- **AdminDashboard**: Panel de administración

#### 8.2.3 Servicios

- **API Service**: Cliente HTTP con interceptores
- **Auth Service**: Servicios de autenticación
- **Student Service**: Servicios de estudiante

### 8.3 Características de UI/UX

- **Diseño Responsivo**: Adaptable a diferentes dispositivos
- **Notificaciones**: Feedback visual con react-toastify
- **Validación de Formularios**: Validación en tiempo real
- **Manejo de Errores**: Visualización amigable de errores

## 9. Despliegue

### 9.1 Requisitos de Infraestructura

- **Servidor Web**: IIS o Kestrel
- **Base de Datos**: SQL Server
- **Servidor de Aplicaciones**: .NET 6 Runtime

### 9.2 Configuración

- **appsettings.json**: Configuración de la aplicación
- **ConnectionStrings**: Cadenas de conexión a la base de datos
- **JWT Settings**: Configuración de tokens JWT

### 9.3 CI/CD

- Integración continua con GitHub Actions
- Despliegue automatizado a Azure App Service
- Pruebas automatizadas antes del despliegue

## 10. Reglas de Negocio

### 10.1 Inscripción de Materias

- Cada estudiante debe inscribirse en exactamente 3 materias
- Un estudiante no puede tener más de una materia con el mismo profesor
- Cada materia equivale a 3 créditos

### 10.2 Acceso a Datos

- Los estudiantes solo pueden ver sus propias inscripciones
- Los estudiantes pueden ver la lista de todos los estudiantes
- Los estudiantes pueden ver compañeros en materias donde están inscritos
- Los administradores tienen acceso completo a todos los datos

## 11. Consideraciones de Rendimiento

- Uso de índices en la base de datos para consultas frecuentes
- Implementación de caché para datos estáticos
- Logging de rendimiento para identificar cuellos de botella
- Paginación en endpoints que devuelven grandes conjuntos de datos

## 12. Futuras Mejoras

- Implementación de autenticación con proveedores externos (Google, Microsoft)
- Sistema de notificaciones en tiempo real
- Exportación de datos a formatos como PDF y Excel
- Implementación de una API GraphQL como alternativa a REST
- Mejora de la cobertura de pruebas automatizadas
- Implementación de un sistema de calificaciones