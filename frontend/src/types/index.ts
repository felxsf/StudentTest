export interface User {
  id: string;
  nombre: string;
  correo: string;
  rol: string;
}

export interface AuthResponse {
  token: string;
  nombre: string;
  correo: string;
  rol: string;
}

export interface LoginRequest {
  correo: string;
  password: string;
}

export interface RegisterRequest {
  nombre: string;
  correo: string;
  password: string;
}

export interface RegisterAdminRequest {
  nombre: string;
  correo: string;
  password: string;
  codigoAdmin: string;
}

export interface Materia {
  id: number;
  nombre: string;
  creditos: number;
  profesorId: number;
  profesorNombre: string;
}

export interface Profesor {
  id: number;
  nombre: string;
  materias: string[];
}

export interface Estudiante {
  id: string;
  nombre: string;
  correo: string;
  materiasInscritas: string[];
}

export interface Inscripcion {
  id: number;
  estudianteId: string;
  estudianteNombre: string;
  estudianteCorreo: string;
  materiaId: number;
  materiaNombre: string;
  materiaCreditos: number;
  profesorId: number;
  profesorNombre: string;
}

export interface InscripcionRequest {
  estudianteId: string;
  materiasIds: number[];
}

export interface Log {
  id: number;
  timeStamp: string;
  level: string;
  message: string;
  exception?: string;
  userId?: string;
  properties?: string;
}

export interface LogsDashboardStats {
  totalLogs: number;
  todayLogs: number;
  totalErrors: number;
  todayErrors: number;
  logsByLevel: Array<{ level: string; count: number }>;
  hourlyLogs: Array<{
    date: string;
    hour: number;
    count: number;
    errors: number;
  }>;
  lastUpdated: string;
}

export interface LogsResponse {
  logs: Log[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
}

export interface LogsSearchFilters {
  level?: string;
  message?: string;
  userId?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
} 