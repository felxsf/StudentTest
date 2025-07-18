import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { useAuth } from '../utils/AuthContext';
import { studentService, publicService } from '../services/api';
import { Estudiante, Materia, Inscripcion } from '../types';
import ClassmatesModal from '../components/ClassmatesModal';

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const [profile, setProfile] = useState<Estudiante | null>(null);
  const [subjects, setSubjects] = useState<Materia[]>([]);
  const [enrollments, setEnrollments] = useState<Inscripcion[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedSubjects, setSelectedSubjects] = useState<number[]>([]);
  const [isEditing, setIsEditing] = useState(false);
  const [stats, setStats] = useState<any>(null);
  const [showClassmatesModal, setShowClassmatesModal] = useState(false);
  const [selectedEnrollment, setSelectedEnrollment] = useState<Inscripcion | null>(null);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      const [profileData, subjectsData, enrollmentsData, studentsData] = await Promise.all([
        studentService.getProfile(),
        publicService.getAllSubjects(),
        studentService.getMyEnrollments(),
        publicService.getAllStudents()
      ]);

      setProfile(profileData);
      setSubjects(subjectsData);
      setEnrollments(enrollmentsData);
      
      // Calcular estadísticas
      setStats({
        totalStudents: studentsData.length,
        totalSubjects: subjectsData.length,
        totalEnrollments: studentsData.reduce((acc, student) => acc + (student.materiasInscritas?.length || 0), 0),
        myEnrollments: enrollmentsData.length
      });
      
      // Si hay inscripciones, pre-seleccionar las materias para edición
      if (enrollmentsData.length > 0) {
        setSelectedSubjects(enrollmentsData.map(e => e.materiaId));
      }
    } catch (error: any) {
      toast.error('Error al cargar los datos del dashboard');
    } finally {
      setLoading(false);
    }
  };

  const handleSubjectSelection = (subjectId: number) => {
    setSelectedSubjects(prev => {
      if (prev.includes(subjectId)) {
        return prev.filter(id => id !== subjectId);
      } else {
        if (prev.length >= 3) {
          toast.warning('Solo puedes seleccionar 3 materias');
          return prev;
        }
        return [...prev, subjectId];
      }
    });
  };

  const handleEnrollment = async () => {
    if (selectedSubjects.length !== 3) {
      toast.error('Debes seleccionar exactamente 3 materias');
      return;
    }

    try {
      if (isEditing) {
        await studentService.updateEnrollment({
          estudianteId: profile?.id || '',
          materiasIds: selectedSubjects,
        });
        toast.success('¡Inscripciones actualizadas exitosamente!');
        setIsEditing(false);
      } else {
        await studentService.enroll({
          estudianteId: profile?.id || '',
          materiasIds: selectedSubjects,
        });
        toast.success('¡Inscripción exitosa!');
      }
      
      loadDashboardData();
    } catch (error: any) {
      const message = error.response?.data?.message || 'Error al procesar la inscripción';
      toast.error(message);
    }
  };

  const handleEditEnrollments = () => {
    setIsEditing(true);
    setSelectedSubjects(enrollments.map(e => e.materiaId));
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
    setSelectedSubjects([]);
  };

  const handleViewClassmates = (enrollment: Inscripcion) => {
    setSelectedEnrollment(enrollment);
    setShowClassmatesModal(true);
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
          Dashboard - {user?.nombre}
        </h1>
        <p className="mt-2 text-gray-600">
          Bienvenido al sistema de registro de estudiantes
        </p>
      </div>

      {/* Stats Cards */}
      {stats && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-blue-100 text-blue-600">
                <span className="text-2xl">👥</span>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Total Estudiantes</p>
                <p className="text-2xl font-semibold text-gray-900">{stats.totalStudents}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-green-100 text-green-600">
                <span className="text-2xl">📚</span>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Materias Disponibles</p>
                <p className="text-2xl font-semibold text-gray-900">{stats.totalSubjects}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-purple-100 text-purple-600">
                <span className="text-2xl">📝</span>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Total Inscripciones</p>
                <p className="text-2xl font-semibold text-gray-900">{stats.totalEnrollments}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-yellow-100 text-yellow-600">
                <span className="text-2xl">🎯</span>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Mis Inscripciones</p>
                <p className="text-2xl font-semibold text-gray-900">{stats.myEnrollments}</p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Profile Card */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-8">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Mi Perfil</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Nombre</label>
            <p className="mt-1 text-sm text-gray-900">{profile?.nombre}</p>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Correo</label>
            <p className="mt-1 text-sm text-gray-900">{profile?.correo}</p>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Materias Inscritas</label>
            <p className="mt-1 text-sm text-gray-900">{profile?.materiasInscritas.length || 0}</p>
          </div>
        </div>
      </div>

      {/* Enrollment Section */}
      {enrollments.length === 0 ? (
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Inscripción en Materias</h2>
          <p className="text-gray-600 mb-4">
            Selecciona exactamente 3 materias para inscribirte. Recuerda que no puedes tener materias con el mismo profesor.
          </p>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
            {subjects.map((subject) => (
              <div
                key={subject.id}
                className={`border-2 rounded-lg p-4 cursor-pointer transition-colors ${
                  selectedSubjects.includes(subject.id)
                    ? 'border-primary-500 bg-primary-50'
                    : 'border-gray-200 hover:border-gray-300'
                }`}
                onClick={() => handleSubjectSelection(subject.id)}
              >
                <h3 className="font-medium text-gray-900">{subject.nombre}</h3>
                <p className="text-sm text-gray-600">Profesor: {subject.profesorNombre}</p>
                <p className="text-sm text-gray-600">Créditos: {subject.creditos}</p>
              </div>
            ))}
          </div>

          <button
            onClick={handleEnrollment}
            disabled={selectedSubjects.length !== 3}
            className="bg-primary-600 text-white px-6 py-2 rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Inscribirse ({selectedSubjects.length}/3)
          </button>
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-semibold text-gray-900">Mis Inscripciones</h2>
            {!isEditing && (
              <button
                onClick={handleEditEnrollments}
                className="bg-yellow-600 text-white px-4 py-2 rounded-md hover:bg-yellow-700"
              >
                Editar Inscripciones
              </button>
            )}
          </div>

          {isEditing ? (
            <>
              <p className="text-gray-600 mb-4">
                Modifica tus materias seleccionadas. Recuerda que debes seleccionar exactamente 3 materias de profesores diferentes.
              </p>
              
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
                {subjects.map((subject) => (
                  <div
                    key={subject.id}
                    className={`border-2 rounded-lg p-4 cursor-pointer transition-colors ${
                      selectedSubjects.includes(subject.id)
                        ? 'border-primary-500 bg-primary-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                    onClick={() => handleSubjectSelection(subject.id)}
                  >
                    <h3 className="font-medium text-gray-900">{subject.nombre}</h3>
                    <p className="text-sm text-gray-600">Profesor: {subject.profesorNombre}</p>
                    <p className="text-sm text-gray-600">Créditos: {subject.creditos}</p>
                  </div>
                ))}
              </div>

              <div className="flex gap-4">
                <button
                  onClick={handleEnrollment}
                  disabled={selectedSubjects.length !== 3}
                  className="bg-primary-600 text-white px-6 py-2 rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Guardar Cambios ({selectedSubjects.length}/3)
                </button>
                <button
                  onClick={handleCancelEdit}
                  className="bg-gray-600 text-white px-6 py-2 rounded-md hover:bg-gray-700"
                >
                  Cancelar
                </button>
              </div>
            </>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {enrollments.map((enrollment) => (
                <div key={enrollment.id} className="border border-gray-200 rounded-lg p-4">
                  <h3 className="font-medium text-gray-900">{enrollment.materiaNombre}</h3>
                  <p className="text-sm text-gray-600">Profesor: {enrollment.profesorNombre}</p>
                  <p className="text-sm text-gray-600">Créditos: {enrollment.materiaCreditos}</p>
                  <button
                    onClick={() => handleViewClassmates(enrollment)}
                    className="mt-3 bg-primary-600 text-white px-3 py-1 rounded-md text-sm hover:bg-primary-700 w-full"
                  >
                    Ver Compañeros
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Available Subjects */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Materias Disponibles</h2>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
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
              {subjects.map((subject) => (
                <tr key={subject.id}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {subject.nombre}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {subject.profesorNombre}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {subject.creditos}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Classmates Modal */}
      {showClassmatesModal && selectedEnrollment && (
        <ClassmatesModal
          materiaId={selectedEnrollment.materiaId}
          materiaNombre={selectedEnrollment.materiaNombre}
          onClose={() => setShowClassmatesModal(false)}
        />
      )}
    </div>
  );
};

export default Dashboard;