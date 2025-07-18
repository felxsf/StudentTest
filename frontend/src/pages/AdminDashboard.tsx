import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { useAuth } from '../utils/AuthContext';
import { adminService } from '../services/api';
import { Estudiante, Profesor, Materia, Inscripcion } from '../types';
import LogsDashboard from '../components/LogsDashboard';

const AdminDashboard: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('dashboard');
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState<any>(null);
  const [students, setStudents] = useState<Estudiante[]>([]);
  const [professors, setProfessors] = useState<Profesor[]>([]);
  const [subjects, setSubjects] = useState<Materia[]>([]);
  const [enrollments, setEnrollments] = useState<Inscripcion[]>([]);

  // Estados para formularios
  const [showAddProfessor, setShowAddProfessor] = useState(false);
  const [showAddSubject, setShowAddSubject] = useState(false);
  const [editingProfessor, setEditingProfessor] = useState<Profesor | null>(null);
  const [editingSubject, setEditingSubject] = useState<Materia | null>(null);
  const [showLogsDashboard, setShowLogsDashboard] = useState(false);

  // Formularios
  const [newProfessor, setNewProfessor] = useState({ nombre: '' });
  const [newSubject, setNewSubject] = useState({ nombre: '', creditos: 3, profesorId: 0 });

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      console.log('Iniciando carga de datos del dashboard...');
      const [statsData, studentsData, professorsData, subjectsData, enrollmentsData] = await Promise.all([
        adminService.getDashboardStats(),
        adminService.getStudents(),
        adminService.getProfessors(),
        adminService.getSubjects(),
        adminService.getEnrollments(),
      ]);

      console.log('Datos de estadísticas recibidos:', statsData);
      setStats(statsData);
      setStudents(studentsData);
      setProfessors(professorsData);
      setSubjects(subjectsData);
      setEnrollments(enrollmentsData);
    } catch (error: any) {
      console.error('Error al cargar los datos del dashboard:', error);
      toast.error('Error al cargar los datos del dashboard');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteStudent = async (studentId: string) => {
    if (window.confirm('¿Estás seguro de que quieres eliminar este estudiante?')) {
      try {
        await adminService.deleteStudent(studentId);
        toast.success('Estudiante eliminado exitosamente');
        loadDashboardData();
      } catch (error: any) {
        toast.error('Error al eliminar el estudiante');
      }
    }
  };

  const handleAddProfessor = async () => {
    try {
      await adminService.addProfessor(newProfessor);
      toast.success('Profesor agregado exitosamente');
      setNewProfessor({ nombre: '' });
      setShowAddProfessor(false);
      loadDashboardData();
    } catch (error: any) {
      toast.error('Error al agregar el profesor');
    }
  };

  const handleUpdateProfessor = async () => {
    if (!editingProfessor) return;
    try {
      await adminService.updateProfessor(editingProfessor.id, { nombre: editingProfessor.nombre });
      toast.success('Profesor actualizado exitosamente');
      setEditingProfessor(null);
      loadDashboardData();
    } catch (error: any) {
      toast.error('Error al actualizar el profesor');
    }
  };

  const handleDeleteProfessor = async (professorId: number) => {
    if (window.confirm('¿Estás seguro de que quieres eliminar este profesor?')) {
      try {
        await adminService.deleteProfessor(professorId);
        toast.success('Profesor eliminado exitosamente');
        loadDashboardData();
      } catch (error: any) {
        toast.error('Error al eliminar el profesor');
      }
    }
  };

  const handleAddSubject = async () => {
    try {
      await adminService.addSubject(newSubject);
      toast.success('Materia agregada exitosamente');
      setNewSubject({ nombre: '', creditos: 3, profesorId: 0 });
      setShowAddSubject(false);
      loadDashboardData();
    } catch (error: any) {
      toast.error('Error al agregar la materia');
    }
  };

  const handleUpdateSubject = async () => {
    if (!editingSubject) return;
    try {
      await adminService.updateSubject(editingSubject.id, {
        nombre: editingSubject.nombre,
        creditos: editingSubject.creditos,
        profesorId: editingSubject.profesorId,
      });
      toast.success('Materia actualizada exitosamente');
      setEditingSubject(null);
      loadDashboardData();
    } catch (error: any) {
      toast.error('Error al actualizar la materia');
    }
  };

  const handleDeleteSubject = async (subjectId: number) => {
    if (window.confirm('¿Estás seguro de que quieres eliminar esta materia?')) {
      try {
        await adminService.deleteSubject(subjectId);
        toast.success('Materia eliminada exitosamente');
        loadDashboardData();
      } catch (error: any) {
        toast.error('Error al eliminar la materia');
      }
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          Panel de Administración - {user?.nombre}
        </h1>
        <p className="mt-2 text-gray-600">
          Gestiona estudiantes, profesores y materias del sistema
        </p>
      </div>

      {/* Navigation Tabs */}
      <div className="border-b border-gray-200 mb-8">
        <nav className="-mb-px flex space-x-8">
          {[
            { id: 'dashboard', name: 'Dashboard', icon: '📊' },
            { id: 'students', name: 'Estudiantes', icon: '👥' },
            { id: 'professors', name: 'Profesores', icon: '👨‍🏫' },
            { id: 'subjects', name: 'Materias', icon: '📚' },
            { id: 'enrollments', name: 'Inscripciones', icon: '📝' },
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === tab.id
                  ? 'border-primary-500 text-primary-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              {tab.icon} {tab.name}
            </button>
          ))}
          <button
            onClick={() => setShowLogsDashboard(true)}
            className="py-2 px-1 border-b-2 font-medium text-sm border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"
          >
            📋 Logs del Sistema
          </button>
        </nav>
      </div>

      {/* Dashboard Stats */}
      {activeTab === 'dashboard' && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-blue-100 text-blue-600">
                <span className="text-2xl">👥</span>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Total Estudiantes</p>
                <p className="text-2xl font-semibold text-gray-900">{stats?.totalEstudiantes || 0}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-green-100 text-green-600">
                <span className="text-2xl">👨‍🏫</span>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Total Profesores</p>
                <p className="text-2xl font-semibold text-gray-900">{stats?.totalProfesores || 0}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-purple-100 text-purple-600">
                <span className="text-2xl">📚</span>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Total Materias</p>
                <p className="text-2xl font-semibold text-gray-900">{stats?.totalMaterias || 0}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-yellow-100 text-yellow-600">
                <span className="text-2xl">📝</span>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Total Inscripciones</p>
                <p className="text-2xl font-semibold text-gray-900">{stats?.totalInscripciones || 0}</p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Students Tab */}
      {activeTab === 'students' && (
        <div className="bg-white rounded-lg shadow-md">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">Gestión de Estudiantes</h2>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Nombre
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Correo
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Materias Inscritas
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {students.map((student) => (
                  <tr key={student.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {student.nombre}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {student.correo}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {student.materiasInscritas?.length || 0}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <button
                        onClick={() => handleDeleteStudent(student.id)}
                        className="text-red-600 hover:text-red-900"
                      >
                        Eliminar
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Professors Tab */}
      {activeTab === 'professors' && (
        <div className="bg-white rounded-lg shadow-md">
          <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
            <h2 className="text-xl font-semibold text-gray-900">Gestión de Profesores</h2>
            <button
              onClick={() => setShowAddProfessor(true)}
              className="bg-primary-600 text-white px-4 py-2 rounded-md hover:bg-primary-700"
            >
              Agregar Profesor
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Nombre
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Materias
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {professors.map((professor) => (
                  <tr key={professor.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {editingProfessor?.id === professor.id ? (
                        <input
                          type="text"
                          value={editingProfessor.nombre}
                          onChange={(e) => setEditingProfessor({ ...editingProfessor, nombre: e.target.value })}
                          className="border border-gray-300 rounded px-2 py-1"
                        />
                      ) : (
                        professor.nombre
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {professor.materias?.join(', ') || 'Ninguna'}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                      {editingProfessor?.id === professor.id ? (
                        <>
                          <button
                            onClick={handleUpdateProfessor}
                            className="text-green-600 hover:text-green-900"
                          >
                            Guardar
                          </button>
                          <button
                            onClick={() => setEditingProfessor(null)}
                            className="text-gray-600 hover:text-gray-900"
                          >
                            Cancelar
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            onClick={() => setEditingProfessor(professor)}
                            className="text-blue-600 hover:text-blue-900"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() => handleDeleteProfessor(professor.id)}
                            className="text-red-600 hover:text-red-900"
                          >
                            Eliminar
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Subjects Tab */}
      {activeTab === 'subjects' && (
        <div className="bg-white rounded-lg shadow-md">
          <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
            <h2 className="text-xl font-semibold text-gray-900">Gestión de Materias</h2>
            <button
              onClick={() => setShowAddSubject(true)}
              className="bg-primary-600 text-white px-4 py-2 rounded-md hover:bg-primary-700"
            >
              Agregar Materia
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Nombre
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Créditos
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Profesor
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {subjects.map((subject) => (
                  <tr key={subject.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {editingSubject?.id === subject.id ? (
                        <input
                          type="text"
                          value={editingSubject.nombre}
                          onChange={(e) => setEditingSubject({ ...editingSubject, nombre: e.target.value })}
                          className="border border-gray-300 rounded px-2 py-1"
                        />
                      ) : (
                        subject.nombre
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {editingSubject?.id === subject.id ? (
                        <input
                          type="number"
                          value={editingSubject.creditos}
                          onChange={(e) => setEditingSubject({ ...editingSubject, creditos: parseInt(e.target.value) })}
                          className="border border-gray-300 rounded px-2 py-1 w-16"
                        />
                      ) : (
                        subject.creditos
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {editingSubject?.id === subject.id ? (
                        <select
                          value={editingSubject.profesorId}
                          onChange={(e) => setEditingSubject({ ...editingSubject, profesorId: parseInt(e.target.value) })}
                          className="border border-gray-300 rounded px-2 py-1"
                        >
                          <option value={0}>Seleccionar profesor</option>
                          {professors.map((prof) => (
                            <option key={prof.id} value={prof.id}>
                              {prof.nombre}
                            </option>
                          ))}
                        </select>
                      ) : (
                        subject.profesorNombre
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                      {editingSubject?.id === subject.id ? (
                        <>
                          <button
                            onClick={handleUpdateSubject}
                            className="text-green-600 hover:text-green-900"
                          >
                            Guardar
                          </button>
                          <button
                            onClick={() => setEditingSubject(null)}
                            className="text-gray-600 hover:text-gray-900"
                          >
                            Cancelar
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            onClick={() => setEditingSubject(subject)}
                            className="text-blue-600 hover:text-blue-900"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() => handleDeleteSubject(subject.id)}
                            className="text-red-600 hover:text-red-900"
                          >
                            Eliminar
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Enrollments Tab */}
      {activeTab === 'enrollments' && (
        <div className="bg-white rounded-lg shadow-md">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">Todas las Inscripciones</h2>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Estudiante
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Materia
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Profesor
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Créditos
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {enrollments.map((enrollment) => (
                  <tr key={enrollment.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {enrollment.estudianteNombre}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {enrollment.materiaNombre}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {enrollment.profesorNombre}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {enrollment.materiaCreditos}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Add Professor Modal */}
      {showAddProfessor && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Agregar Profesor</h3>
              <input
                type="text"
                placeholder="Nombre del profesor"
                value={newProfessor.nombre}
                onChange={(e) => setNewProfessor({ nombre: e.target.value })}
                className="w-full border border-gray-300 rounded px-3 py-2 mb-4"
              />
              <div className="flex justify-end space-x-2">
                <button
                  onClick={() => setShowAddProfessor(false)}
                  className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleAddProfessor}
                  className="bg-primary-600 text-white px-4 py-2 rounded hover:bg-primary-700"
                >
                  Agregar
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Add Subject Modal */}
      {showAddSubject && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Agregar Materia</h3>
              <input
                type="text"
                placeholder="Nombre de la materia"
                value={newSubject.nombre}
                onChange={(e) => setNewSubject({ ...newSubject, nombre: e.target.value })}
                className="w-full border border-gray-300 rounded px-3 py-2 mb-4"
              />
              <input
                type="number"
                placeholder="Créditos"
                value={newSubject.creditos}
                onChange={(e) => setNewSubject({ ...newSubject, creditos: parseInt(e.target.value) })}
                className="w-full border border-gray-300 rounded px-3 py-2 mb-4"
              />
              <select
                value={newSubject.profesorId}
                onChange={(e) => setNewSubject({ ...newSubject, profesorId: parseInt(e.target.value) })}
                className="w-full border border-gray-300 rounded px-3 py-2 mb-4"
              >
                <option value={0}>Seleccionar profesor</option>
                {professors.map((prof) => (
                  <option key={prof.id} value={prof.id}>
                    {prof.nombre}
                  </option>
                ))}
              </select>
              <div className="flex justify-end space-x-2">
                <button
                  onClick={() => setShowAddSubject(false)}
                  className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleAddSubject}
                  className="bg-primary-600 text-white px-4 py-2 rounded hover:bg-primary-700"
                >
                  Agregar
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Logs Dashboard */}
      {showLogsDashboard && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative min-h-screen">
            <LogsDashboard onClose={() => setShowLogsDashboard(false)} />
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminDashboard; 