# StudentTest Frontend

Frontend en React para el Sistema de Registro de Estudiantes.

## 🚀 Características

- **Diseño Responsivo**: Optimizado para dispositivos móviles y desktop
- **Autenticación JWT**: Sistema de login seguro con tokens
- **Notificaciones**: Feedback visual con react-toastify
- **Navegación Intuitiva**: Menú responsive con hamburger en móviles
- **Gestión de Estado**: Context API para autenticación
- **TypeScript**: Tipado fuerte para mejor desarrollo
- **Tailwind CSS**: Estilos modernos y responsivos

## 📱 Diseño Responsivo

La aplicación se adapta perfectamente a diferentes tamaños de pantalla:

- **Mobile First**: Diseño optimizado para móviles
- **Tablet**: Layout adaptado para tablets
- **Desktop**: Experiencia completa en pantallas grandes
- **Navegación**: Menú hamburger en móviles, menú horizontal en desktop

## 🎨 Interactividad y Feedback

### Notificaciones
- ✅ **Éxito**: Mensajes verdes para acciones exitosas
- ❌ **Error**: Mensajes rojos para errores
- ⚠️ **Advertencia**: Mensajes amarillos para advertencias
- ℹ️ **Información**: Mensajes azules para información

### Estados de Carga
- **Spinners**: Indicadores de carga durante peticiones
- **Botones Deshabilitados**: Prevención de múltiples envíos
- **Skeleton Loading**: Placeholders durante carga de datos

### Validaciones
- **Formularios**: Validación en tiempo real
- **Contraseñas**: Confirmación de contraseñas
- **Campos Requeridos**: Indicadores visuales

## 🛠️ Tecnologías Utilizadas

- **React 18**: Framework principal
- **TypeScript**: Tipado estático
- **React Router**: Navegación SPA
- **Axios**: Cliente HTTP
- **React Toastify**: Notificaciones
- **Tailwind CSS**: Framework de estilos
- **Headless UI**: Componentes accesibles

## 📦 Instalación

1. **Instalar dependencias**:
```bash
npm install
```

2. **Iniciar servidor de desarrollo**:
```bash
npm start
```

3. **Construir para producción**:
```bash
npm run build
```

## 🔧 Configuración

### Variables de Entorno
Crea un archivo `.env` en la raíz del proyecto:

```env
REACT_APP_API_URL=https://localhost:7259/api
```

### Conexión con Backend
El frontend está configurado para conectarse con tu API .NET en:
- **URL**: `https://localhost:7259/api`
- **Autenticación**: JWT Bearer Token
- **CORS**: Configurado para desarrollo local

## 📱 Páginas y Funcionalidades

### 🔐 Autenticación
- **Login**: Formulario de inicio de sesión
- **Registro**: Formulario de registro de estudiantes
- **Protección de Rutas**: Acceso controlado por roles

### 🎓 Dashboard de Estudiante
- **Perfil**: Información personal del estudiante
- **Inscripción**: Selección de 3 materias
- **Materias Disponibles**: Lista de todas las materias
- **Mis Inscripciones**: Materias en las que está inscrito

### 📊 Funcionalidades
- **Selección de Materias**: Interfaz visual para seleccionar materias
- **Validaciones**: Verificación de reglas de negocio
- **Notificaciones**: Feedback inmediato de acciones
- **Navegación**: Menú responsive con logout

## 🎨 Componentes Principales

### Layout
- **Navigation**: Barra de navegación responsive
- **ProtectedRoute**: Protección de rutas
- **AuthProvider**: Contexto de autenticación

### Páginas
- **Login**: Formulario de autenticación
- **Register**: Formulario de registro
- **Dashboard**: Panel principal del estudiante

### Servicios
- **API Service**: Cliente HTTP con interceptores
- **Auth Service**: Servicios de autenticación
- **Student Service**: Servicios de estudiante

## 📱 Responsive Design

### Breakpoints
- **Mobile**: < 768px
- **Tablet**: 768px - 1024px
- **Desktop**: > 1024px

### Características Responsivas
- **Grid System**: Adaptable a diferentes pantallas
- **Typography**: Escalado automático
- **Spacing**: Márgenes y padding responsivos
- **Navigation**: Menú hamburger en móviles

## 🔒 Seguridad

- **JWT Tokens**: Autenticación segura
- **Interceptores**: Manejo automático de tokens
- **Logout Automático**: En caso de token expirado
- **Validación de Roles**: Control de acceso por roles

## 🚀 Despliegue

### Desarrollo
```bash
npm start
```

### Producción
```bash
npm run build
```

Los archivos de producción se generan en la carpeta `build/`.

## 📝 Scripts Disponibles

- `npm start`: Inicia el servidor de desarrollo
- `npm run build`: Construye la aplicación para producción
- `npm test`: Ejecuta las pruebas
- `npm run eject`: Expone la configuración de webpack

## 🔗 Integración con Backend

El frontend está completamente integrado con tu API .NET:

1. **Autenticación**: Login y registro funcionando
2. **Gestión de Materias**: Listado y selección
3. **Inscripciones**: Proceso completo de inscripción
4. **Perfil**: Información del estudiante
5. **Notificaciones**: Feedback de todas las operaciones

## 🎯 Próximos Pasos

1. **Ejecutar el frontend**: `npm start`
2. **Asegurar que el backend esté corriendo** en `https://localhost:7259`
3. **Probar el flujo completo**: Registro → Login → Inscripción
4. **Verificar responsividad** en diferentes dispositivos

¡Tu aplicación está lista para usar! 🎉 