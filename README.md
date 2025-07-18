# Sistema de Registro de Estudiantes

## Descripción
Sistema de registro de estudiantes para inscripción de materias con autenticación JWT y autorización por roles. Implementado con arquitectura limpia (Clean Architecture) y siguiendo buenas prácticas de desarrollo.

## Documentación

- [Documentación Técnica Completa](./DOCUMENTACION_TECNICA.md) - Detalles de arquitectura, tecnologías y componentes
- [Documentación Frontend](./frontend/README.md) - Detalles específicos del frontend

## Requisitos Funcionales Implementados

✅ **CRUD de Estudiantes**: Registro en línea para inscripción de materias  
✅ **Sistema de Créditos**: Cada materia equivale a 3 créditos  
✅ **10 Materias Disponibles**: Configuradas automáticamente  
✅ **Límite de 3 Materias**: Los estudiantes solo pueden seleccionar 3 materias  
✅ **5 Profesores**: Cada uno imparte 2 materias  
✅ **Restricción de Profesores**: Un estudiante no puede tener clases con el mismo profesor en más de una materia  
✅ **Visualización de Registros**: Los estudiantes pueden ver registros de otros estudiantes  
✅ **Compañeros por Clase**: Los estudiantes pueden ver compañeros en cada materia  
✅ **Autenticación JWT**: Sistema de autenticación seguro  
✅ **Autorización por Roles**: Control de acceso basado en roles (Estudiante/Admin)  
✅ **Logging Avanzado**: Sistema de registro de eventos y auditoría  
✅ **Manejo de Excepciones**: Sistema centralizado de manejo de errores  
✅ **Pruebas Unitarias**: Cobertura de pruebas para componentes clave

## Tecnologías Utilizadas

### Backend
- **ASP.NET Core 6**: Framework principal
- **Entity Framework Core**: ORM para acceso a datos
- **SQL Server**: Base de datos relacional
- **JWT**: Autenticación basada en tokens
- **Serilog**: Logging estructurado
- **xUnit**: Framework de pruebas unitarias

### Frontend
- **React 18**: Framework principal
- **TypeScript**: Tipado estático
- **React Router**: Navegación SPA
- **Axios**: Cliente HTTP
- **Tailwind CSS**: Framework de estilos

## Estructura del Proyecto

```
StudentTest/
├── API/                 # Capa de presentación (Controllers, Program.cs)
├── Application/         # Capa de aplicación (Services, DTOs, Interfaces)
├── Domain/              # Capa de dominio (Entities)
├── Infrastructure/      # Capa de infraestructura (DbContext, Repositories)
├── Tests/               # Pruebas unitarias
└── frontend/            # Aplicación React
```

## Endpoints de la API

### 🔐 Autenticación (Sin autorización requerida)

#### 1. Registro de Estudiante
```http
POST /api/Estudiantes/registro
Content-Type: application/json

{
  "nombre": "Juan Pérez",
  "correo": "juan@email.com",
  "password": "123456"
}
```

#### 2. Registro de Administrador
```http
POST /api/Estudiantes/registro-admin
Content-Type: application/json

{
  "nombre": "Admin Principal",
  "correo": "admin@email.com",
  "password": "admin123",
  "codigoAdmin": "ADMIN2024"
}
```

#### 3. Login
```http
POST /api/Estudiantes/login
Content-Type: application/json

{
  "correo": "juan@email.com",
  "password": "123456"
}
```

**Respuesta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "nombre": "Juan Pérez",
  "correo": "juan@email.com",
  "rol": "Estudiante"
}
```

### 👨‍🎓 Endpoints para Estudiantes (Requieren token JWT)

#### 4. Inscripción en Materias
```http
POST /api/Estudiantes/inscripcion
Authorization: Bearer {token}
Content-Type: application/json

{
  "estudianteId": "GUID_DEL_ESTUDIANTE",
  "materiasIds": [1, 2, 3]
}
```

#### 5. Ver Mi Perfil
```http
GET /api/Estudiantes/mi-perfil
Authorization: Bearer {token}
```

#### 6. Ver Mis Inscripciones
```http
GET /api/Estudiantes/mis-inscripciones
Authorization: Bearer {token}
```

#### 7. Ver Mis Compañeros en una Materia
```http
GET /api/Estudiantes/mis-compañeros/{materiaId}
Authorization: Bearer {token}
```

### 📚 Endpoints Públicos (Requieren token JWT)

#### 8. Ver Todas las Materias
```http
GET /api/Estudiantes/materias
Authorization: Bearer {token}
```

#### 9. Ver Todos los Profesores
```http
GET /api/Estudiantes/profesores
Authorization: Bearer {token}
```

#### 10. Ver Todos los Estudiantes
```http
GET /api/Estudiantes/estudiantes
Authorization: Bearer {token}
```

#### 11. Ver Compañeros de una Materia
```http
GET /api/Estudiantes/companeromateria/{materiaId}
Authorization: Bearer {token}
```

### 👨‍💼 Endpoints para Administradores

#### 12. Dashboard de Administrador
```http
GET /api/Estudiantes/admin/dashboard
Authorization: Bearer {token}
```

#### 13. Ver Todas las Inscripciones
```http
GET /api/Estudiantes/inscripciones
Authorization: Bearer {token}
```

## Reglas de Negocio

### Para Estudiantes:
- ✅ Solo pueden inscribirse en exactamente 3 materias
- ✅ No pueden tener materias con el mismo profesor
- ✅ Solo pueden ver sus propias inscripciones y perfil
- ✅ Pueden ver la lista de todos los estudiantes
- ✅ Pueden ver compañeros en materias donde están inscritos

### Para Administradores:
- ✅ Acceso completo a todos los endpoints
- ✅ Pueden ver todas las inscripciones
- ✅ Pueden crear nuevos administradores

## Configuración de la Base de Datos

La aplicación usa Azure SQL Database con la siguiente cadena de conexión:
```
Server=tcp:.database.windows.net,1433;
Initial Catalog=StudentTestDB;
User ID=;
Password=;
Encrypt=True;
TrustServerCertificate=False;
```

## Datos Iniciales

Al iniciar la aplicación, se crean automáticamente:

### Profesores:
1. Dr. Carlos Mendoza
2. Dra. Ana García
3. Dr. Luis Rodríguez
4. Dra. María López
5. Dr. Roberto Silva

### Materias (3 créditos cada una):
1. Matemáticas I (Prof. Carlos Mendoza)
2. Física I (Prof. Carlos Mendoza)
3. Programación I (Prof. Ana García)
4. Bases de Datos (Prof. Ana García)
5. Inglés Técnico (Prof. Luis Rodríguez)
6. Comunicación (Prof. Luis Rodríguez)
7. Estadística (Prof. María López)
8. Investigación (Prof. María López)
9. Ética Profesional (Prof. Roberto Silva)
10. Gestión de Proyectos (Prof. Roberto Silva)

## Instalación y Ejecución

### Backend

1. Clonar el repositorio
```bash
git clone https://github.com/tu-usuario/StudentTest.git
cd StudentTest
```

2. Restaurar paquetes NuGet
```bash
dotnet restore
```

3. Actualizar la base de datos
```bash
cd Infrastructure
dotnet ef database update
```

4. Ejecutar la API
```bash
cd ../API
dotnet run
```

### Frontend

1. Navegar al directorio frontend
```bash
cd ../frontend
```

2. Instalar dependencias
```bash
npm install
```

3. Ejecutar en modo desarrollo
```bash
npm start
```

## Pruebas

Ejecutar pruebas unitarias:
```bash
cd ../Tests
dotnet test
```

## Contribución

1. Hacer fork del repositorio
2. Crear una rama para tu feature (`git checkout -b feature/amazing-feature`)
3. Commit de tus cambios (`git commit -m 'Add some amazing feature'`)
4. Push a la rama (`git push origin feature/amazing-feature`)
5. Abrir un Pull Request

## Licencia

Este proyecto está licenciado bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

## Contacto

Felix Rafael Sanchez - felix_sanchez@hotmail.com

Enlace del proyecto: [https://github.com/felxsf/StudentTest](https://github.com/tu-usuario/StudentTest)
