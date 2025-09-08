import React, { useState } from 'react';
import { X, User } from 'lucide-react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { InputField } from '../forms/InputField';
import { SelectField } from '../forms/SelectField';
import { LoadingSpinner } from '../common/LoadingSpinner';
import { personsApi } from '../../services/api';
import { CreatePersonRequest, PersonResponseDto } from '../../types/api.types';
import { validateCPF } from '../../utils/validation';
import { formatCPF, formatDate, formatDateForAPI, formatPhone } from '../../utils/formatters';

interface AddPersonModalProps {
  isOpen: boolean;
  onClose: () => void;
  onPersonAdded: (person: PersonResponseDto) => void;
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

export const AddPersonModal: React.FC<AddPersonModalProps> = ({
  isOpen,
  onClose,
  onPersonAdded,
}) => {
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    reset,
  } = useForm<PersonFormData>();

  React.useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
      reset(); // Reset form when modal opens
    } else {
      document.body.style.overflow = 'unset';
    }

    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, reset]);

  if (!isOpen) {
    return null;
  }

  const onSubmit = async (data: PersonFormData) => {
    try {
      setIsLoading(true);

      const personData: CreatePersonRequest = {
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

      const newPerson = await personsApi.create(personData);
      onPersonAdded(newPerson);
      toast.success('Pessoa criada com sucesso!');
      onClose();
    } catch (error: any) {
      console.error('Error creating person:', error);
      if (error.status === 409) {
        toast.error('CPF já está cadastrado no sistema');
      } else if (error.status === 400) {
        toast.error('Dados inválidos. Verifique as informações e tente novamente.');
      } else if (error.status === 403) {
        toast.error('Você não tem permissão para criar pessoas');
      } else if (error.message?.includes('Erro de conexão')) {
        toast.error('Erro de conexão. Verifique sua internet e tente novamente.');
      } else {
        toast.error('Erro ao criar pessoa. Tente novamente.');
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
            <div className="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
              <User className="w-5 h-5 text-green-600" />
            </div>
            <h2 className="text-xl font-semibold text-gray-900">Adicionar Nova Pessoa</h2>
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
              {...register('email', {
                pattern: {
                  value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                  message: 'Email inválido',
                },
              })}
            />

            <InputField
              label="Naturalidade (opcional)"
              placeholder="Cidade de nascimento"
              error={errors.naturalidade?.message}
              {...register('naturalidade')}
            />

            <InputField
              label="Nacionalidade (opcional)"
              placeholder="ex: Brasileira"
              error={errors.nacionalidade?.message}
              {...register('nacionalidade')}
            />

            <InputField
              label="Telefone (opcional)"
              placeholder="(00) 0000-0000"
              maxLength={14}
              error={errors.telefone?.message}
              {...register('telefone', {
                onChange: handlePhoneChange,
              })}
            />

            <InputField
              label="Celular (opcional)"
              placeholder="(00) 00000-0000"
              maxLength={15}
              error={errors.celular?.message}
              {...register('celular', {
                onChange: handleCellPhoneChange,
              })}
            />
          </div>

          {/* Actions */}
          <div className="flex items-center gap-3 pt-6 border-t border-gray-200">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              disabled={isLoading}
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isLoading}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <>
                  <LoadingSpinner size="sm" className="text-white" />
                  Criando...
                </>
              ) : (
                'Criar Pessoa'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
