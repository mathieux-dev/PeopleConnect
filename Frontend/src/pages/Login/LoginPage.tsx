import React, { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Users, LogIn } from 'lucide-react';
import toast from 'react-hot-toast';
import { useAuth } from '../../hooks/useAuth';
import { InputField } from '../../components/forms/InputField';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { validateUsername } from '../../utils/validation';

interface LoginFormData {
  username: string;
  password: string;
}

export const LoginPage: React.FC = () => {
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const from = location.state?.from?.pathname || '/persons';

  const {
    register,
    handleSubmit,
    formState: { errors },
    setError,
  } = useForm<LoginFormData>();

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    
    try {
      await login(data.username, data.password);
      navigate(from, { replace: true });
    } catch (error: any) {
      console.error('Error during login:', error);
      if (error.status === 401) {
        setError('username', { message: 'Usuário ou senha incorretos' });
        setError('password', { message: 'Usuário ou senha incorretos' });
      } else if (error.message?.includes('Erro de conexão')) {
        toast.error('Erro de conexão. Verifique sua internet e tente novamente.');
      } else {
        toast.error('Erro ao fazer login. Tente novamente.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 px-4">
      <div className="max-w-md w-full">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
            <Users className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">PeopleConnect</h1>
          <p className="text-gray-600">Entre na sua conta para continuar</p>
        </div>

        {/* Login Card */}
        <div className="bg-white rounded-2xl shadow-xl p-8">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
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
                minLength: {
                  value: 6,
                  message: 'Senha deve ter pelo menos 6 caracteres',
                },
              })}
            />

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold py-3 px-4 rounded-lg transition-colors duration-200 flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <LoadingSpinner size="sm" />
              ) : (
                <>
                  <LogIn className="w-5 h-5" />
                  Entrar
                </>
              )}
            </button>
          </form>

          {/* Register Link */}
          <div className="mt-6 text-center">
            <p className="text-gray-600">
              Não tem uma conta?{' '}
              <Link 
                to="/register" 
                className="text-blue-600 hover:text-blue-700 font-semibold transition-colors duration-200"
              >
                Criar Conta
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
