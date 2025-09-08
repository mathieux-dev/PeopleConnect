import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Users, UserPlus, ArrowLeft } from 'lucide-react';
import toast from 'react-hot-toast';
import { useAuth } from '../../hooks/useAuth';
import { InputField } from '../../components/forms/InputField';
import { SelectField } from '../../components/forms/SelectField';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { validateUsername, validateCPF, validateEmail, validatePassword } from '../../utils/validation';
import { formatCPF, formatDate, formatDateForAPI, formatPhone } from '../../utils/formatters';
import { RegisterRequest } from '../../types/api.types';

interface RegisterFormData {
  username: string;
  password: string;
  confirmPassword: string;
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

export const RegisterPage: React.FC = () => {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<RegisterFormData>();

  const password = watch('password');

  const onSubmit = async (data: RegisterFormData) => {
    setIsLoading(true);
    
    try {
      const registerData: RegisterRequest = {
        username: data.username,
        password: data.password,
        person: {
          nome: data.nome,
          cpf: data.cpf.replace(/\D/g, ''), // Remove formatting
          dataNascimento: formatDateForAPI(data.dataNascimento),
          email: data.email || undefined,
          sexo: data.sexo !== undefined ? data.sexo : undefined,
          naturalidade: data.naturalidade || undefined,
          nacionalidade: data.nacionalidade || undefined,
          telefone: data.telefone || undefined,
          celular: data.celular || undefined,
        },
      };

      await registerUser(registerData);
      navigate('/persons', { replace: true });
    } catch (error: any) {
      console.error('Error registering user:', error);
      if (error.status === 409) {
        toast.error('Usuário ou CPF já cadastrado');
      } else if (error.status === 400) {
        toast.error('Dados inválidos. Verifique as informações e tente novamente.');
      } else if (error.message?.includes('Erro de conexão')) {
        toast.error('Erro de conexão. Verifique sua internet e tente novamente.');
      } else {
        toast.error('Erro ao criar conta. Tente novamente.');
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
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 px-4 py-8">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
            <Users className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">PeopleConnect</h1>
          <p className="text-gray-600">Crie sua conta para começar</p>
        </div>

        {/* Register Card */}
        <div className="bg-white rounded-2xl shadow-xl p-8">
          {/* Back Button */}
          <Link 
            to="/login"
            className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-800 mb-6 transition-colors duration-200"
          >
            <ArrowLeft className="w-4 h-4" />
            Voltar ao login
          </Link>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
            {/* Account Section */}
            <div>
              <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <UserPlus className="w-5 h-5" />
                Dados da Conta
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <InputField
                  label="Nome de usuário"
                  type="text"
                  placeholder="Digite seu usuário"
                  error={errors.username?.message}
                  {...register('username', {
                    required: 'Nome de usuário é obrigatório',
                    validate: (value) => 
                      validateUsername(value) || 'Usuário deve ter 3-20 caracteres (letras, números e _)',
                  })}
                />

                <InputField
                  label="Senha"
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Digite sua senha"
                  error={errors.password?.message}
                  showPasswordToggle
                  showPassword={showPassword}
                  onTogglePassword={() => setShowPassword(!showPassword)}
                  {...register('password', {
                    required: 'Senha é obrigatória',
                    validate: (value) => {
                      const validation = validatePassword(value);
                      return validation.isValid || validation.errors.join(', ');
                    },
                  })}
                />

                <div className="md:col-span-2">
                  <InputField
                    label="Confirmar senha"
                    type={showConfirmPassword ? 'text' : 'password'}
                    placeholder="Confirme sua senha"
                    error={errors.confirmPassword?.message}
                    showPasswordToggle
                    showPassword={showConfirmPassword}
                    onTogglePassword={() => setShowConfirmPassword(!showConfirmPassword)}
                    {...register('confirmPassword', {
                      required: 'Confirmação de senha é obrigatória',
                      validate: (value) => 
                        value === password || 'As senhas não coincidem',
                    })}
                  />
                </div>
              </div>
            </div>

            {/* Personal Data Section */}
            <div>
              <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <Users className="w-5 h-5" />
                Dados Pessoais
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="md:col-span-2">
                  <InputField
                    label="Nome completo"
                    type="text"
                    placeholder="Digite seu nome completo"
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
                  type="text"
                  placeholder="000.000.000-00"
                  maxLength={14}
                  error={errors.cpf?.message}
                  {...register('cpf', {
                    required: 'CPF é obrigatório',
                    validate: (value) => 
                      validateCPF(value) || 'CPF inválido',
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
                    validate: (value) => 
                      !value || validateEmail(value) || 'Email inválido',
                  })}
                />

                <InputField
                  label="Telefone (opcional)"
                  type="text"
                  placeholder="(11) 1234-5678"
                  maxLength={15}
                  error={errors.telefone?.message}
                  {...register('telefone', {
                    onChange: handlePhoneChange,
                  })}
                />

                <InputField
                  label="Celular (opcional)"
                  type="text"
                  placeholder="(11) 91234-5678"
                  maxLength={15}
                  error={errors.celular?.message}
                  {...register('celular', {
                    onChange: handleCellPhoneChange,
                  })}
                />

                <InputField
                  label="Naturalidade (opcional)"
                  type="text"
                  placeholder="Cidade de nascimento"
                  error={errors.naturalidade?.message}
                  {...register('naturalidade')}
                />

                <InputField
                  label="Nacionalidade (opcional)"
                  type="text"
                  placeholder="País de origem"
                  error={errors.nacionalidade?.message}
                  {...register('nacionalidade')}
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold py-3 px-4 rounded-lg transition-colors duration-200 flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <LoadingSpinner size="sm" />
              ) : (
                <>
                  <UserPlus className="w-5 h-5" />
                  Criar Conta
                </>
              )}
            </button>
          </form>

          {/* Login Link */}
          <div className="mt-6 text-center">
            <p className="text-gray-600">
              Já tem uma conta?{' '}
              <Link 
                to="/login" 
                className="text-blue-600 hover:text-blue-700 font-semibold transition-colors duration-200"
              >
                Entrar
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
