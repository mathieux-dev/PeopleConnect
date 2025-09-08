import React, { useState, useEffect } from 'react';
import { X, User } from 'lucide-react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { InputField } from '../forms/InputField';
import { SelectField } from '../forms/SelectField';
import { LoadingSpinner } from '../common/LoadingSpinner';
import { personsApi } from '../../services/api';
import { PersonResponseDto, UpdatePersonRequest } from '../../types/api.types';
import { validateCPF } from '../../utils/validation';
import { formatCPF, formatDate, formatDateForAPI, formatPhone, formatDateFromAPI } from '../../utils/formatters';

interface EditPersonModalProps {
  person: PersonResponseDto | null;
  isOpen: boolean;
  onClose: () => void;
  onPersonUpdated: (person: PersonResponseDto) => void;
}

interface PersonFormData {
  nome: string;
  cpf: string;
  dataNascimento: string;
  email?: string;
  sexo?: 'M' | 'F' | '';
  naturalidade?: string;
  nacionalidade?: string;
  telefone?: string;
  celular?: string;
}

export const EditPersonModal: React.FC<EditPersonModalProps> = ({
  person,
  isOpen,
  onClose,
  onPersonUpdated,
}) => {
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    reset,
  } = useForm<PersonFormData>();

  useEffect(() => {
    if (isOpen && person) {
      // Populate form with person data
      reset({
        nome: person.nome,
        cpf: person.cpf,
        dataNascimento: formatDateFromAPI(person.dataNascimento),
        email: person.email || '',
        sexo: person.sexo || '',
        naturalidade: person.naturalidade || '',
        nacionalidade: person.nacionalidade || '',
        telefone: person.contacts?.find(c => c.type === 'Telefone')?.value || '',
        celular: person.contacts?.find(c => c.type === 'Celular')?.value || '',
      });
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }

    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, person, reset]);

  if (!isOpen || !person) {
    return null;
  }

  const onSubmit = async (data: PersonFormData) => {
    try {
      setIsLoading(true);

      const updateData: UpdatePersonRequest = {
        nome: data.nome,
        cpf: data.cpf.replace(/\D/g, ''), // Remove formatting
        dataNascimento: formatDateForAPI(data.dataNascimento),
        email: data.email || undefined,
        sexo: data.sexo !== undefined ? data.sexo : undefined,
        naturalidade: data.naturalidade || undefined,
        nacionalidade: data.nacionalidade || undefined,
        telefone: data.telefone || undefined,
        celular: data.celular || undefined,
      };

      const updatedPerson = await personsApi.update(person.id, updateData);
      onPersonUpdated(updatedPerson);
      toast.success('Pessoa atualizada com sucesso!');
      onClose();
    } catch (error: any) {
      console.error('Error updating person:', error);
      if (error.status === 409) {
        toast.error('CPF já está cadastrado no sistema');
      } else if (error.status === 400) {
        toast.error('Dados inválidos. Verifique as informações e tente novamente.');
      } else if (error.status === 403) {
        toast.error('Você não tem permissão para editar pessoas');
      } else if (error.status === 404) {
        toast.error('Pessoa não encontrada');
      } else if (error.message?.includes('Erro de conexão')) {
        toast.error('Erro de conexão. Verifique sua internet e tente novamente.');
      } else {
        toast.error('Erro ao atualizar pessoa. Tente novamente.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleCPFChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatCPF(e.target.value);
    setValue('cpf', formatted);
  };

  const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatDate(e.target.value);
    setValue('dataNascimento', formatted);
  };

  const handlePhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatPhone(e.target.value);
    setValue('telefone', formatted);
  };

  const handleCellPhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatPhone(e.target.value);
    setValue('celular', formatted);
  };

  const sexOptions = [
    { value: '', label: 'Não Informado' },
    { value: 'M', label: 'Masculino' },
    { value: 'F', label: 'Feminino' },
  ];

  return (
    <div 
      className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
      onClick={(e) => {
        if (e.target === e.currentTarget) {
          onClose();
        }
      }}
    >
      <div className="bg-white rounded-lg max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
              <User className="w-5 h-5 text-blue-600" />
            </div>
            <h2 className="text-xl font-semibold text-gray-900">Editar Pessoa</h2>
          </div>
          <button
            onClick={onClose}
            className="w-8 h-8 flex items-center justify-center rounded-full hover:bg-gray-100 transition-colors"
          >
            <X className="w-5 h-5 text-gray-500" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="md:col-span-2">
              <InputField
                label="Nome completo"
                placeholder="Digite o nome completo"
                error={errors.nome?.message}
                {...register('nome', {
                  required: 'Nome é obrigatório',
                  minLength: {
                    value: 2,
                    message: 'Nome deve ter pelo menos 2 caracteres',
                  },
                })}
              />
            </div>

            <InputField
              label="CPF"
              placeholder="000.000.000-00"
              maxLength={14}
              error={errors.cpf?.message}
              {...register('cpf', {
                required: 'CPF é obrigatório',
                validate: (value) => {
                  const cleanCPF = value.replace(/\D/g, '');
                  return validateCPF(cleanCPF) || 'CPF inválido';
                },
                onChange: handleCPFChange,
              })}
            />

            <InputField
              label="Data de nascimento"
              type="text"
              placeholder="DD/MM/AAAA"
              maxLength={10}
              error={errors.dataNascimento?.message}
              {...register('dataNascimento', {
                required: 'Data de nascimento é obrigatória',
                pattern: {
                  value: /^\d{2}\/\d{2}\/\d{4}$/,
                  message: 'Data deve estar no formato DD/MM/AAAA',
                },
                onChange: handleDateChange,
              })}
            />

            <SelectField
              label="Sexo (opcional)"
              options={sexOptions}
              error={errors.sexo?.message}
              {...register('sexo')}
            />

            <InputField
              label="Email (opcional)"
              type="email"
              placeholder="seu@email.com"
              error={errors.email?.message}
              {...register('email')}
            />

            <InputField
              label="Naturalidade (opcional)"
              placeholder="Cidade de nascimento"
              error={errors.naturalidade?.message}
              {...register('naturalidade')}
            />

            <InputField
              label="Nacionalidade (opcional)"
              placeholder="País de origem"
              error={errors.nacionalidade?.message}
              {...register('nacionalidade')}
            />

            <InputField
              label="Telefone (opcional)"
              placeholder="(11) 1234-5678"
              maxLength={14}
              error={errors.telefone?.message}
              {...register('telefone', {
                onChange: handlePhoneChange,
              })}
            />

            <InputField
              label="Celular (opcional)"
              placeholder="(11) 91234-5678"
              maxLength={15}
              error={errors.celular?.message}
              {...register('celular', {
                onChange: handleCellPhoneChange,
              })}
            />
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2 text-gray-600 hover:text-gray-800 font-medium transition-colors duration-200"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isLoading}
              className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-medium rounded-lg transition-colors duration-200 flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <LoadingSpinner size="sm" />
              ) : (
                'Salvar Alterações'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
