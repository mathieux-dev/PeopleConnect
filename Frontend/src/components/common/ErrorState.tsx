import React from 'react';
import { AlertCircle, RefreshCw, WifiOff } from 'lucide-react';

interface ErrorStateProps {
  title?: string;
  message?: string;
  type?: 'error' | 'network' | 'notFound' | 'forbidden';
  onRetry?: () => void;
  retryLabel?: string;
  className?: string;
}

export const ErrorState: React.FC<ErrorStateProps> = ({
  title,
  message,
  type = 'error',
  onRetry,
  retryLabel = 'Tentar novamente',
  className = ''
}) => {
  const getConfig = () => {
    switch (type) {
      case 'network':
        return {
          icon: WifiOff,
          defaultTitle: 'Erro de Conexão',
          defaultMessage: 'Não foi possível conectar ao servidor. Verifique sua conexão com a internet.',
          iconColor: 'text-orange-500',
          bgColor: 'bg-orange-50'
        };
      case 'notFound':
        return {
          icon: AlertCircle,
          defaultTitle: 'Não Encontrado',
          defaultMessage: 'O recurso que você está procurando não foi encontrado.',
          iconColor: 'text-gray-500',
          bgColor: 'bg-gray-50'
        };
      case 'forbidden':
        return {
          icon: AlertCircle,
          defaultTitle: 'Acesso Negado',
          defaultMessage: 'Você não tem permissão para acessar este recurso.',
          iconColor: 'text-yellow-500',
          bgColor: 'bg-yellow-50'
        };
      default:
        return {
          icon: AlertCircle,
          defaultTitle: 'Algo deu errado',
          defaultMessage: 'Ocorreu um erro inesperado. Tente novamente em alguns momentos.',
          iconColor: 'text-red-500',
          bgColor: 'bg-red-50'
        };
    }
  };

  const config = getConfig();
  const Icon = config.icon;

  return (
    <div className={`text-center py-12 ${className}`}>
      <div className={`w-16 h-16 ${config.bgColor} rounded-full flex items-center justify-center mx-auto mb-4`}>
        <Icon className={`w-8 h-8 ${config.iconColor}`} />
      </div>
      <h3 className="text-lg font-medium text-gray-900 mb-2">
        {title || config.defaultTitle}
      </h3>
      <p className="text-gray-600 mb-6 max-w-md mx-auto">
        {message || config.defaultMessage}
      </p>
      {onRetry && (
        <button
          onClick={onRetry}
          className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors duration-200"
        >
          <RefreshCw className="w-4 h-4" />
          {retryLabel}
        </button>
      )}
    </div>
  );
};

interface ErrorBannerProps {
  message: string;
  type?: 'error' | 'warning' | 'info';
  onClose?: () => void;
  className?: string;
}

export const ErrorBanner: React.FC<ErrorBannerProps> = ({
  message,
  type = 'error',
  onClose,
  className = ''
}) => {
  const getStyles = () => {
    switch (type) {
      case 'warning':
        return {
          bg: 'bg-yellow-50',
          border: 'border-yellow-200',
          text: 'text-yellow-800',
          icon: 'text-yellow-500'
        };
      case 'info':
        return {
          bg: 'bg-blue-50',
          border: 'border-blue-200',
          text: 'text-blue-800',
          icon: 'text-blue-500'
        };
      default:
        return {
          bg: 'bg-red-50',
          border: 'border-red-200',
          text: 'text-red-800',
          icon: 'text-red-500'
        };
    }
  };

  const styles = getStyles();

  return (
    <div className={`
      ${styles.bg} ${styles.border} border rounded-lg p-4 mb-4 ${className}
    `}>
      <div className="flex items-center gap-3">
        <AlertCircle className={`w-5 h-5 ${styles.icon} flex-shrink-0`} />
        <p className={`${styles.text} flex-1`}>{message}</p>
        {onClose && (
          <button
            onClick={onClose}
            className={`${styles.text} hover:opacity-75 transition-opacity`}
          >
            ×
          </button>
        )}
      </div>
    </div>
  );
};
