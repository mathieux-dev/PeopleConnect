import React, { useState } from 'react';
import { X, User, Trash2, AlertTriangle, Edit } from 'lucide-react';
import toast from 'react-hot-toast';
import { PersonResponseDto } from '../../types/api.types';
import { formatDateFromAPI, getContactIcon } from '../../utils/formatters';
import { useAuth } from '../../hooks/useAuth';
import { personsApi } from '../../services/api';
import { LoadingSpinner } from '../common/LoadingSpinner';

interface PersonDetailsModalProps {
  person: PersonResponseDto | null;
  isOpen: boolean;
  onClose: () => void;
  onPersonDeleted: (personId: string) => void;
  onPersonEdit?: (person: PersonResponseDto) => void;
}

export const PersonDetailsModal: React.FC<PersonDetailsModalProps> = ({
  person,
  isOpen,
  onClose,
  onPersonDeleted,
  onPersonEdit,
}) => {
  const { isAdmin } = useAuth();
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  React.useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }

    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  if (!person) {
    return null;
  }

  const handleDelete = async () => {
    if (!person) return;
    
    setIsDeleting(true);
    try {
      await personsApi.delete(person.id);
      toast.success('Pessoa excluída com sucesso!');
      onPersonDeleted(person.id);
      onClose();
    } catch (error: any) {
      console.error('Error deleting person:', error);
      if (error.status === 403) {
        toast.error('Você não tem permissão para excluir pessoas');
      } else if (error.status === 404) {
        toast.error('Pessoa não encontrada');
      } else if (error.message?.includes('Erro de conexão')) {
        toast.error('Erro de conexão. Verifique sua internet e tente novamente.');
      } else {
        toast.error('Erro ao excluir pessoa. Tente novamente.');
      }
    } finally {
      setIsDeleting(false);
      setShowDeleteConfirm(false);
    }
  };

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div 
      className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 backdrop-blur-sm p-4"
      onClick={handleBackdropClick}
    >
      <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto animate-in fade-in zoom-in duration-300">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
              <User className="w-5 h-5 text-blue-600" />
            </div>
            <h2 className="text-xl font-semibold text-gray-900">Detalhes da Pessoa</h2>
          </div>
          <button
            onClick={onClose}
            className="w-8 h-8 flex items-center justify-center rounded-full hover:bg-gray-100 transition-colors duration-200"
          >
            <X className="w-5 h-5 text-gray-500" />
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-6">
          {/* Personal Information */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Dados Pessoais</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-500 mb-1">Nome</label>
                <p className="text-gray-900 font-medium">{person.nome}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-500 mb-1">CPF</label>
                <p className="text-gray-900 font-mono">{person.cpf}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-500 mb-1">Data de Nascimento</label>
                <p className="text-gray-900">{formatDateFromAPI(person.dataNascimento)}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-500 mb-1">Sexo</label>
                <p className="text-gray-900">{person.sexo === 'M' ? 'Masculino' : person.sexo === 'F' ? 'Feminino' : 'Não Informado'}</p>
              </div>
              {person.email && (
                <div>
                  <label className="block text-sm font-medium text-gray-500 mb-1">Email</label>
                  <p className="text-gray-900">{person.email}</p>
                </div>
              )}
              {person.naturalidade && (
                <div>
                  <label className="block text-sm font-medium text-gray-500 mb-1">Naturalidade</label>
                  <p className="text-gray-900">{person.naturalidade}</p>
                </div>
              )}
              {person.nacionalidade && (
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-gray-500 mb-1">Nacionalidade</label>
                  <p className="text-gray-900">{person.nacionalidade}</p>
                </div>
              )}
            </div>
          </div>

          {/* Contacts */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Contatos</h3>
            {person.contacts && person.contacts.length > 0 ? (
              <div className="space-y-3">
                {person.contacts.map((contact) => (
                  <div 
                    key={contact.id} 
                    className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg"
                  >
                    <span className="text-xl">{getContactIcon(contact.type)}</span>
                    <div>
                      <p className="font-medium text-gray-900">{contact.type}</p>
                      <p className="text-gray-600">{contact.value}</p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8 bg-gray-50 rounded-lg">
                <div className="w-12 h-12 bg-gray-200 rounded-full flex items-center justify-center mx-auto mb-3">
                  <User className="w-6 h-6 text-gray-400" />
                </div>
                <p className="text-gray-500 font-medium mb-1">Nenhum contato encontrado</p>
                <p className="text-gray-400 text-sm">Esta pessoa ainda não possui informações de contato cadastradas.</p>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-6 border-t border-gray-200">
          <div className="flex gap-3">
            {isAdmin && onPersonEdit && (
              <button
                onClick={() => onPersonEdit(person)}
                className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors duration-200"
              >
                <Edit className="w-4 h-4" />
                Editar
              </button>
            )}
            {isAdmin && (
              <button
                onClick={() => setShowDeleteConfirm(true)}
                disabled={isDeleting}
                className="flex items-center gap-2 px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-red-400 text-white font-medium rounded-lg transition-colors duration-200"
              >
                {isDeleting ? (
                  <LoadingSpinner size="sm" />
                ) : (
                  <Trash2 className="w-4 h-4" />
                )}
                Excluir Pessoa
              </button>
            )}
          </div>
          <button
            onClick={onClose}
            className="px-6 py-2 text-gray-600 hover:text-gray-800 font-medium transition-colors duration-200"
          >
            Fechar
          </button>
        </div>

        {/* Delete Confirmation Modal */}
        {showDeleteConfirm && (
          <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-xl p-6 max-w-md w-full">
              <div className="flex items-center gap-3 mb-4">
                <div className="w-10 h-10 bg-red-100 rounded-full flex items-center justify-center">
                  <AlertTriangle className="w-5 h-5 text-red-600" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900">Confirmar Exclusão</h3>
              </div>
              <p className="text-gray-600 mb-6">
                Tem certeza que deseja excluir <strong>{person.nome}</strong>? 
                Esta ação não pode ser desfeita.
              </p>
              <div className="flex gap-3">
                <button
                  onClick={() => setShowDeleteConfirm(false)}
                  className="flex-1 px-4 py-2 text-gray-600 hover:text-gray-800 font-medium transition-colors duration-200"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleDelete}
                  disabled={isDeleting}
                  className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-red-400 text-white font-medium rounded-lg transition-colors duration-200 flex items-center justify-center gap-2"
                >
                  {isDeleting ? (
                    <LoadingSpinner size="sm" />
                  ) : (
                    <>
                      <Trash2 className="w-4 h-4" />
                      Excluir
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
