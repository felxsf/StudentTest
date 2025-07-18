import axios from 'axios';
import { 
  AuthResponse, 
  LoginRequest, 
  RegisterRequest, 
  RegisterAdminRequest,
  Materia,
  Profesor,
  Estudiante,
  Inscripcion,
  InscripcionRequest
} from '../types';

const API_BASE_URL = 'https://localhost:7259/api';

// Configurar axios con interceptores para el token
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true // Importante para CORS con credenciales
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
    console.error('Error en la petición:', error);
    return Promise.reject(error);
  }
);

// Interceptor para manejar errores de respuesta
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error.response?.data || error.message);
    
    if (error.response?.status === 401) {
      console.log('Error de autenticación, redirigiendo a login...');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
      return Promise.reject(new Error('Sesión expirada. Por favor, inicie sesión nuevamente.'));
    }
    
    if (error.response?.status === 403) {
      return Promise.reject(new Error('No tiene permisos para realizar esta acción.'));
    }

    if (error.code === 'ERR_NETWORK') {
      return Promise.reject(new Error('Error de conexión. Por favor, verifique su conexión a internet.'));
    }

    return Promise.reject(error.response?.data || error);
  }
);

// Servicios de autenticación
export const authService = {
  login: async (credentials: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/Estudiantes/login', credentials);
    return response.data;
  },

  register: async (userData: RegisterRequest): Promise<{ id: string; mensaje: string }> => {
    const response = await api.post<{ id: string; mensaje: string }>('/Estudiantes/registro', userData);
    return response.data;
  },

  registerAdmin: async (adminData: RegisterAdminRequest): Promise<{ id: string; mensaje: string }> => {
    const response = await api.post<{ id: string; mensaje: string }>('/Estudiantes/registro-admin', adminData);
    return response.data;
  },
};

// Servicios de estudiantes
export const studentService = {
  getProfile: async (): Promise<Estudiante> => {
    const response = await api.get<Estudiante>('/Estudiantes/mi-perfil');
    return response.data;
  },

  getMyEnrollments: async (): Promise<Inscripcion[]> => {
    const response = await api.get<Inscripcion[]>('/Estudiantes/mis-inscripciones');
    return response.data;
  },

  getMyClassmates: async (materiaId: number): Promise<string[]> => {
    const response = await api.get<string[]>(`/Estudiantes/mis-compañeros/${materiaId}`);
    return response.data;
  },

  enroll: async (enrollmentData: InscripcionRequest): Promise<{ mensaje: string }> => {
    const response = await api.post<{ mensaje: string }>('/Estudiantes/inscripcion', enrollmentData);
    return response.data;
  },

  updateEnrollment: async (enrollmentData: InscripcionRequest): Promise<{ mensaje: string }> => {
    const response = await api.put<{ mensaje: string }>('/Estudiantes/inscripcion', enrollmentData);
    return response.data;
  },
};

// Servicios públicos
export const publicService = {
  getAllStudents: async (): Promise<Estudiante[]> => {
    const response = await api.get<Estudiante[]>('/Estudiantes/estudiantes');
    return response.data;
  },

  getAllSubjects: async (): Promise<Materia[]> => {
    const response = await api.get<Materia[]>('/Estudiantes/materias');
    return response.data;
  },

  getAllProfessors: async (): Promise<Profesor[]> => {
    const response = await api.get<Profesor[]>('/Estudiantes/profesores');
    return response.data;
  },

  getClassmatesBySubject: async (materiaId: number): Promise<string[]> => {
    const response = await api.get<string[]>(`/Estudiantes/companeromateria/${materiaId}`);
    return response.data;
  },
};

// Servicios de administrador
export const adminService = {
  getDashboardStats: async (): Promise<any> => {
    console.log('Realizando llamada a /Admin/dashboard-stats...');
    try {
      const response = await api.get('/Admin/dashboard-stats');
      console.log('Respuesta recibida de dashboard-stats:', response.data);
      return response.data;
    } catch (error) {
      console.error('Error en getDashboardStats:', error);
      throw error;
    }
  },

  // Gestión de estudiantes
  getStudents: async (): Promise<Estudiante[]> => {
    const response = await api.get<Estudiante[]>('/Admin/estudiantes');
    return response.data;
  },

  deleteStudent: async (studentId: string): Promise<{ mensaje: string }> => {
    const response = await api.delete<{ mensaje: string }>(`/Admin/estudiantes/${studentId}`);
    return response.data;
  },

  // Gestión de profesores
  getProfessors: async (): Promise<Profesor[]> => {
    const response = await api.get<Profesor[]>('/Admin/profesores');
    return response.data;
  },

  addProfessor: async (professor: { nombre: string }): Promise<Profesor> => {
    const response = await api.post<Profesor>('/Admin/profesores', professor);
    return response.data;
  },

  updateProfessor: async (professorId: number, professor: { nombre: string }): Promise<Profesor> => {
    const response = await api.put<Profesor>(`/Admin/profesores/${professorId}`, professor);
    return response.data;
  },

  deleteProfessor: async (professorId: number): Promise<{ mensaje: string }> => {
    const response = await api.delete<{ mensaje: string }>(`/Admin/profesores/${professorId}`);
    return response.data;
  },

  // Gestión de materias
  getSubjects: async (): Promise<Materia[]> => {
    const response = await api.get<Materia[]>('/Admin/materias');
    return response.data;
  },

  addSubject: async (subject: { nombre: string; creditos: number; profesorId: number }): Promise<Materia> => {
    const response = await api.post<Materia>('/Admin/materias', subject);
    return response.data;
  },

  updateSubject: async (subjectId: number, subject: { nombre: string; creditos: number; profesorId: number }): Promise<Materia> => {
    const response = await api.put<Materia>(`/Admin/materias/${subjectId}`, subject);
    return response.data;
  },

  deleteSubject: async (subjectId: number): Promise<{ mensaje: string }> => {
    const response = await api.delete<{ mensaje: string }>(`/Admin/materias/${subjectId}`);
    return response.data;
  },

  // Gestión de inscripciones
  getEnrollments: async (): Promise<Inscripcion[]> => {
    const response = await api.get<Inscripcion[]>('/Admin/inscripciones');
    return response.data;
  },

  // Gestión de logs
  getLogsDashboard: async (): Promise<any> => {
    const response = await api.get('/logs/dashboard');
    return response.data;
  },

  getRecentLogs: async (page: number = 1, pageSize: number = 50): Promise<any> => {
    const response = await api.get(`/logs/recent?page=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  getErrorLogs: async (page: number = 1, pageSize: number = 50): Promise<any> => {
    const response = await api.get(`/logs/errors?page=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  getPerformanceLogs: async (page: number = 1, pageSize: number = 50): Promise<any> => {
    const response = await api.get(`/logs/performance?page=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  getSecurityLogs: async (page: number = 1, pageSize: number = 50): Promise<any> => {
    const response = await api.get(`/logs/security?page=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  searchLogs: async (filters: {
    level?: string;
    message?: string;
    userId?: string;
    startDate?: string;
    endDate?: string;
    page?: number;
    pageSize?: number;
  }): Promise<any> => {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value) params.append(key, String(value));
    });
    const response = await api.get(`/logs/search?${params}`);
    return response.data;
  },

  exportLogs: async (filters: {
    startDate?: string;
    endDate?: string;
    level?: string;
  }): Promise<Blob> => {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value) params.append(key, String(value));
    });
    const response = await api.get(`/logs/export?${params}`, {
      responseType: 'blob'
    });
    return response.data;
  },

  cleanupLogs: async (daysToKeep: number = 30): Promise<any> => {
    const response = await api.delete(`/logs/cleanup?daysToKeep=${daysToKeep}`);
    return response.data;
  },
};

export default api; 