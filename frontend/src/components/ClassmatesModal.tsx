import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { studentService } from '../services/api';

interface ClassmatesModalProps {
  materiaId: number;
  materiaNombre: string;
  onClose: () => void;
}

const ClassmatesModal: React.FC<ClassmatesModalProps> = ({ materiaId, materiaNombre, onClose }) => {
  const [classmates, setClassmates] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadClassmates = async () => {
      try {
        setLoading(true);
        const data = await studentService.getMyClassmates(materiaId);
        setClassmates(data);
      } catch (error: any) {
        console.error('Error loading classmates:', error);
        toast.error(error.message || 'Error al cargar compañeros');
      } finally {
        setLoading(false);
      }
    };

    loadClassmates();
  }, [materiaId]);

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
      <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
        <div className="mt-3">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-lg font-medium text-gray-900">Compañeros en {materiaNombre}</h3>
            <button
              onClick={onClose}
              className="text-gray-500 hover:text-gray-700"
            >
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          
          {loading ? (
            <div className="flex justify-center py-4">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
          ) : classmates.length > 0 ? (
            <ul className="divide-y divide-gray-200">
              {classmates.map((classmate, index) => (
                <li key={index} className="py-3 flex items-center">
                  <div className="flex-shrink-0 h-8 w-8 rounded-full bg-primary-100 flex items-center justify-center text-primary-600">
                    {classmate.charAt(0).toUpperCase()}
                  </div>
                  <div className="ml-3">
                    <p className="text-sm font-medium text-gray-900">{classmate}</p>
                  </div>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-center text-gray-500 py-4">No hay compañeros inscritos en esta materia.</p>
          )}
          
          <div className="mt-4 flex justify-end">
            <button
              onClick={onClose}
              className="bg-primary-600 text-white px-4 py-2 rounded hover:bg-primary-700"
            >
              Cerrar
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ClassmatesModal;