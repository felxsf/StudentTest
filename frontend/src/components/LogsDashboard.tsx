import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { adminService } from '../services/api';
import { Log, LogsDashboardStats, LogsResponse, LogsSearchFilters } from '../types';

interface LogsDashboardProps {
  onClose?: () => void;
}

const LogsDashboard: React.FC<LogsDashboardProps> = ({ onClose }) => {
  const [activeTab, setActiveTab] = useState('dashboard');
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState<LogsDashboardStats | null>(null);
  const [logs, setLogs] = useState<Log[]>([]);
  const [pagination, setPagination] = useState<any>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);

  // Estados para búsqueda
  const [searchFilters, setSearchFilters] = useState<LogsSearchFilters>({
    level: '',
    message: '',
    userId: '',
    startDate: '',
    endDate: '',
    page: 1,
    pageSize: 50
  });

  const loadDashboardData = useCallback(async () => {
    try {
      setLoading(true);
      console.log('Cargando datos del dashboard...');
      const data = await adminService.getLogsDashboard();
      console.log('Datos recibidos:', data);
      setStats(data);
    } catch (error: any) {
      console.error('Error loading dashboard data:', error);
      toast.error(error.message || 'Error al cargar estadísticas del dashboard');
      if (error.message?.includes('Sesión expirada')) {
        // La redirección ya está manejada en el servicio de API
        return;
      }
    } finally {
      setLoading(false);
    }
  }, []);

  const loadLogs = useCallback(async () => {
    try {
      setLoading(true);
      console.log(`Cargando logs para tab: ${activeTab}`);
      let data: LogsResponse;

      switch (activeTab) {
        case 'recent':
          data = await adminService.getRecentLogs(currentPage, pageSize);
          break;
        case 'errors':
          data = await adminService.getErrorLogs(currentPage, pageSize);
          break;
        case 'performance':
          data = await adminService.getPerformanceLogs(currentPage, pageSize);
          break;
        case 'security':
          data = await adminService.getSecurityLogs(currentPage, pageSize);
          break;
        case 'search':
          data = await adminService.searchLogs({
            ...searchFilters,
            page: currentPage,
            pageSize
          });
          break;
        default:
          data = await adminService.getRecentLogs(currentPage, pageSize);
      }

      console.log('Logs recibidos:', data);
      setLogs(data.logs);
      setPagination(data.pagination);
    } catch (error: any) {
      console.error('Error loading logs:', error);
      toast.error(error.message || 'Error al cargar logs');
      if (error.message?.includes('Sesión expirada')) {
        return;
      }
    } finally {
      setLoading(false);
    }
  }, [activeTab, currentPage, pageSize, searchFilters]);

  useEffect(() => {
    loadDashboardData();
  }, [loadDashboardData]);

  useEffect(() => {
    if (activeTab === 'dashboard') {
      loadDashboardData();
    } else {
      loadLogs();
    }
  }, [activeTab, currentPage, pageSize, loadDashboardData, loadLogs]);

  const handleSearch = useCallback(() => {
    setCurrentPage(1);
    // La búsqueda se ejecutará automáticamente por el useEffect
  }, []);

  const handleExport = useCallback(async () => {
    try {
      toast.info('Preparando exportación...');
      
      const blob = await adminService.exportLogs({
        startDate: searchFilters.startDate,
        endDate: searchFilters.endDate,
        level: searchFilters.level
      });

      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `logs_export_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);

      toast.success('Logs exportados exitosamente');
    } catch (error: any) {
      console.error('Error exporting logs:', error);
      toast.error('Error al exportar logs: ' + (error.message || 'Error desconocido'));
    }
  }, [searchFilters]);

  const handleCleanup = useCallback(async () => {
    if (window.confirm('¿Estás seguro de que quieres limpiar logs antiguos? Esto eliminará logs de más de 30 días.')) {
      try {
        toast.info('Iniciando limpieza de logs...');
        await adminService.cleanupLogs(30);
        toast.success('Limpieza de logs completada');
        // Recargar datos del dashboard
        await loadDashboardData();
        // Si no estamos en dashboard, recargar logs actuales
        if (activeTab !== 'dashboard') {
          await loadLogs();
        }
      } catch (error: any) {
        console.error('Error cleaning up logs:', error);
        toast.error('Error al limpiar logs: ' + (error.message || 'Error desconocido'));
      }
    }
  }, [loadDashboardData, loadLogs, activeTab]);

  const handleTabChange = useCallback((tabId: string) => {
    setActiveTab(tabId);
    setCurrentPage(1); // Resetear a la primera página al cambiar de tab
  }, []);

  const handlePageSizeChange = useCallback((newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1); // Resetear a la primera página al cambiar el tamaño
  }, []);

  const handlePageChange = useCallback((newPage: number) => {
    setCurrentPage(newPage);
  }, []);

  const getLevelColor = (level: string) => {
    switch (level.toLowerCase()) {
      case 'error':
      case 'fatal':
        return 'bg-red-100 text-red-800 border-red-200';
      case 'warning':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'information':
        return 'bg-blue-100 text-blue-800 border-blue-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const getLevelIcon = (level: string) => {
    switch (level.toLowerCase()) {
      case 'error':
      case 'fatal':
        return '🔴';
      case 'warning':
        return '🟡';
      case 'information':
        return '🔵';
      default:
        return '⚪';
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('es-ES');
  };

  if (loading && !stats) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-4 border-blue-600 border-t-transparent mx-auto mb-4"></div>
          <p className="text-gray-600 font-medium">Cargando dashboard de logs...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-gradient-to-br from-gray-50 to-blue-50 min-h-screen">
      {/* Header con gradiente */}
      <div className="bg-gradient-to-r from-blue-600 via-purple-600 to-indigo-600 text-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-3xl font-bold flex items-center">
                <svg className="w-8 h-8 mr-3" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clipRule="evenodd" />
                </svg>
                Dashboard de Logs del Sistema
              </h1>
              <p className="mt-2 text-blue-100">
                Monitoreo y gestión avanzada de logs del sistema StudentTest
              </p>
            </div>
            <div className="flex space-x-3">
              <button
                onClick={handleExport}
                disabled={loading}
                className="bg-green-500 hover:bg-green-600 disabled:bg-gray-400 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center"
              >
                <svg className="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M3 17a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm3.293-7.707a1 1 0 011.414 0L9 10.586V3a1 1 0 112 0v7.586l1.293-1.293a1 1 0 111.414 1.414l-3 3a1 1 0 01-1.414 0l-3-3a1 1 0 010-1.414z" clipRule="evenodd" />
                </svg>
                Exportar
              </button>
              <button
                onClick={handleCleanup}
                disabled={loading}
                className="bg-orange-500 hover:bg-orange-600 disabled:bg-gray-400 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center"
              >
                <svg className="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                </svg>
                Limpiar
              </button>
              {onClose && (
                <button
                  onClick={onClose}
                  className="bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded-lg font-medium transition-colors duration-200 flex items-center"
                >
                  <svg className="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                  </svg>
                  Cerrar
                </button>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Navigation Tabs mejorados */}
        <div className="bg-white rounded-xl shadow-lg p-2 mb-8">
          <nav className="flex space-x-1">
            {[
              { id: 'dashboard', name: 'Dashboard', icon: '📊', color: 'from-blue-500 to-blue-600' },
              { id: 'recent', name: 'Recientes', icon: '🕒', color: 'from-green-500 to-green-600' },
              { id: 'errors', name: 'Errores', icon: '⚠️', color: 'from-red-500 to-red-600' },
              { id: 'performance', name: 'Rendimiento', icon: '⚡', color: 'from-purple-500 to-purple-600' },
              { id: 'security', name: 'Seguridad', icon: '🔒', color: 'from-indigo-500 to-indigo-600' },
              { id: 'search', name: 'Búsqueda', icon: '🔍', color: 'from-orange-500 to-orange-600' },
            ].map((tab) => (
              <button
                key={tab.id}
                onClick={() => handleTabChange(tab.id)}
                disabled={loading}
                className={`flex-1 py-3 px-4 rounded-lg font-medium text-sm transition-all duration-200 ${
                  activeTab === tab.id
                    ? `bg-gradient-to-r ${tab.color} text-white shadow-md transform scale-105`
                    : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100'
                } ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
              >
                <span className="text-lg mr-2">{tab.icon}</span>
                {tab.name}
              </button>
            ))}
          </nav>
        </div>

        {/* Dashboard Stats mejorados */}
        {activeTab === 'dashboard' && stats && (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
              <div className="bg-gradient-to-br from-blue-500 to-blue-600 rounded-xl shadow-lg p-6 text-white transform hover:scale-105 transition-transform duration-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-blue-100 text-sm font-medium">Total de Logs</p>
                    <p className="text-3xl font-bold">{stats.totalLogs.toLocaleString()}</p>
                  </div>
                  <div className="bg-blue-400 bg-opacity-30 rounded-full p-3">
                    <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clipRule="evenodd" />
                    </svg>
                  </div>
                </div>
              </div>

              <div className="bg-gradient-to-br from-green-500 to-green-600 rounded-xl shadow-lg p-6 text-white transform hover:scale-105 transition-transform duration-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-green-100 text-sm font-medium">Logs Hoy</p>
                    <p className="text-3xl font-bold">{stats.todayLogs.toLocaleString()}</p>
                  </div>
                  <div className="bg-green-400 bg-opacity-30 rounded-full p-3">
                    <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clipRule="evenodd" />
                    </svg>
                  </div>
                </div>
              </div>

              <div className="bg-gradient-to-br from-red-500 to-red-600 rounded-xl shadow-lg p-6 text-white transform hover:scale-105 transition-transform duration-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-red-100 text-sm font-medium">Total Errores</p>
                    <p className="text-3xl font-bold">{stats.totalErrors.toLocaleString()}</p>
                  </div>
                  <div className="bg-red-400 bg-opacity-30 rounded-full p-3">
                    <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                  </div>
                </div>
              </div>

              <div className="bg-gradient-to-br from-orange-500 to-orange-600 rounded-xl shadow-lg p-6 text-white transform hover:scale-105 transition-transform duration-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-orange-100 text-sm font-medium">Errores Hoy</p>
                    <p className="text-3xl font-bold">{stats.todayErrors.toLocaleString()}</p>
                  </div>
                  <div className="bg-orange-400 bg-opacity-30 rounded-full p-3">
                    <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                  </div>
                </div>
              </div>
            </div>

            {/* Charts Row mejorados */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
              <div className="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
                <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
                  <svg className="w-6 h-6 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M2 11a1 1 0 011-1h2a1 1 0 011 1v5a1 1 0 01-1 1H3a1 1 0 01-1-1v-5zM8 7a1 1 0 011-1h2a1 1 0 011 1v9a1 1 0 01-1 1H9a1 1 0 01-1-1V7zM14 4a1 1 0 011-1h2a1 1 0 011 1v12a1 1 0 01-1 1h-2a1 1 0 01-1-1V4z" />
                  </svg>
                  Logs por Nivel
                </h3>
                <div className="space-y-4">
                  {stats.logsByLevel.map((item, index) => (
                    <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                      <div className="flex items-center">
                        <span className="text-2xl mr-3">{getLevelIcon(item.level)}</span>
                        <span className={`font-semibold ${getLevelColor(item.level)} px-3 py-1 rounded-full text-xs border`}>
                          {item.level}
                        </span>
                      </div>
                      <span className="text-lg font-bold text-gray-900">{item.count.toLocaleString()}</span>
                    </div>
                  ))}
                </div>
              </div>

              <div className="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
                <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
                  <svg className="w-6 h-6 mr-2 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M3 3a1 1 0 000 2v8a2 2 0 002 2h2.586l-1.293 1.293a1 1 0 101.414 1.414L10 15.414l2.293 2.293a1 1 0 001.414-1.414L12.414 15H15a2 2 0 002-2V5a1 1 0 100-2H3zm11.707 4.707a1 1 0 00-1.414-1.414L10 9.586 8.707 8.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                  Actividad por Hora (Últimas 24h)
                </h3>
                <div className="space-y-3">
                  {stats.hourlyLogs.slice(-12).map((item, index) => (
                    <div key={index} className="flex justify-between items-center p-3 bg-gray-50 rounded-lg">
                      <span className="text-sm font-medium text-gray-700">
                        {new Date(item.date).toLocaleDateString()} {item.hour}:00
                      </span>
                      <div className="flex space-x-4">
                        <span className="text-blue-600 font-bold">{item.count}</span>
                        <span className="text-red-600 font-bold">{item.errors}</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </>
        )}

        {/* Search Filters mejorados */}
        {activeTab === 'search' && (
          <div className="bg-white rounded-xl shadow-lg p-6 mb-6 border border-gray-100">
            <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
              <svg className="w-6 h-6 mr-2 text-orange-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
              </svg>
              Filtros de Búsqueda Avanzada
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Nivel de Log</label>
                <select
                  value={searchFilters.level}
                  onChange={(e) => setSearchFilters({ ...searchFilters, level: e.target.value })}
                  className="w-full border border-gray-300 rounded-lg px-4 py-3 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
                >
                  <option value="">Todos los niveles</option>
                  <option value="Information">Information</option>
                  <option value="Warning">Warning</option>
                  <option value="Error">Error</option>
                  <option value="Fatal">Fatal</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">ID de Usuario</label>
                <input
                  type="text"
                  value={searchFilters.userId}
                  onChange={(e) => setSearchFilters({ ...searchFilters, userId: e.target.value })}
                  placeholder="Buscar por usuario"
                  className="w-full border border-gray-300 rounded-lg px-4 py-3 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
                />
              </div>

              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Mensaje</label>
                <input
                  type="text"
                  value={searchFilters.message}
                  onChange={(e) => setSearchFilters({ ...searchFilters, message: e.target.value })}
                  placeholder="Buscar en mensajes"
                  className="w-full border border-gray-300 rounded-lg px-4 py-3 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
                />
              </div>

              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Fecha de Inicio</label>
                <input
                  type="datetime-local"
                  value={searchFilters.startDate}
                  onChange={(e) => setSearchFilters({ ...searchFilters, startDate: e.target.value })}
                  className="w-full border border-gray-300 rounded-lg px-4 py-3 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
                />
              </div>

              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">Fecha de Fin</label>
                <input
                  type="datetime-local"
                  value={searchFilters.endDate}
                  onChange={(e) => setSearchFilters({ ...searchFilters, endDate: e.target.value })}
                  className="w-full border border-gray-300 rounded-lg px-4 py-3 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
                />
              </div>

              <div className="flex items-end">
                <button
                  onClick={handleSearch}
                  disabled={loading}
                  className="w-full bg-gradient-to-r from-blue-600 to-blue-700 text-white px-6 py-3 rounded-lg font-semibold hover:from-blue-700 hover:to-blue-800 disabled:from-gray-400 disabled:to-gray-500 transition-all duration-200 transform hover:scale-105 flex items-center justify-center"
                >
                  <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
                  </svg>
                  Buscar Logs
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Logs Table mejorado */}
        {activeTab !== 'dashboard' && (
          <div className="bg-white rounded-xl shadow-lg border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200 bg-gradient-to-r from-gray-50 to-gray-100">
              <div className="flex justify-between items-center">
                <h2 className="text-xl font-bold text-gray-900 flex items-center">
                  {activeTab === 'recent' && (
                    <>
                      <svg className="w-6 h-6 mr-2 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z" clipRule="evenodd" />
                      </svg>
                      Logs Recientes
                    </>
                  )}
                  {activeTab === 'errors' && (
                    <>
                      <svg className="w-6 h-6 mr-2 text-red-600" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                      </svg>
                      Logs de Errores
                    </>
                  )}
                  {activeTab === 'performance' && (
                    <>
                      <svg className="w-6 h-6 mr-2 text-purple-600" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clipRule="evenodd" />
                      </svg>
                      Logs de Rendimiento
                    </>
                  )}
                  {activeTab === 'security' && (
                    <>
                      <svg className="w-6 h-6 mr-2 text-indigo-600" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
                      </svg>
                      Logs de Seguridad
                    </>
                  )}
                  {activeTab === 'search' && (
                    <>
                      <svg className="w-6 h-6 mr-2 text-orange-600" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
                      </svg>
                      Resultados de Búsqueda
                    </>
                  )}
                </h2>
                <div className="flex items-center space-x-4">
                  <select
                    value={pageSize}
                    onChange={(e) => handlePageSizeChange(Number(e.target.value))}
                    disabled={loading}
                    className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:opacity-50"
                  >
                    <option value={25}>25 por página</option>
                    <option value={50}>50 por página</option>
                    <option value={100}>100 por página</option>
                  </select>
                </div>
              </div>
            </div>

            {loading ? (
              <div className="p-12 text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-600 border-t-transparent mx-auto mb-4"></div>
                <p className="text-gray-600 font-medium">Cargando logs...</p>
              </div>
            ) : (
              <>
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Nivel
                        </th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Fecha/Hora
                        </th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Mensaje
                        </th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Usuario
                        </th>
                        <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Propiedades
                        </th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {logs.map((log) => (
                        <tr key={log.id} className="hover:bg-gray-50">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getLevelColor(log.level)}`}>
                              {getLevelIcon(log.level)} {log.level}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {formatDate(log.timeStamp)}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-900">
                            <div className="max-w-lg break-words">
                              {log.message}
                              {log.exception && (
                                <div className="mt-1 text-red-600 text-xs">
                                  {log.exception}
                                </div>
                              )}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {log.userId || '-'}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {log.properties || '-'}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                {/* Pagination mejorado */}
                {pagination && (
                  <div className="px-6 py-4 border-t border-gray-200 bg-gray-50">
                    <div className="flex justify-between items-center">
                      <div className="text-sm text-gray-700 font-medium">
                        Mostrando {((pagination.currentPage - 1) * pagination.pageSize) + 1} -{' '}
                        {Math.min(pagination.currentPage * pagination.pageSize, pagination.totalCount)} de{' '}
                        {pagination.totalCount.toLocaleString()} logs
                      </div>
                      <div className="flex space-x-2">
                        <button
                          onClick={() => handlePageChange(pagination.currentPage - 1)}
                          disabled={!pagination.hasPreviousPage || loading}
                          className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100 transition-colors flex items-center"
                        >
                          <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
                          </svg>
                          Anterior
                        </button>
                        <span className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg">
                          Página {pagination.currentPage} de {pagination.totalPages}
                        </span>
                        <button
                          onClick={() => handlePageChange(pagination.currentPage + 1)}
                          disabled={!pagination.hasNextPage || loading}
                          className="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100 transition-colors flex items-center"
                        >
                          Siguiente
                          <svg className="w-4 h-4 ml-1" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
                          </svg>
                        </button>
                      </div>
                    </div>
                  </div>
                )}
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default LogsDashboard; 