import React, { forwardRef, useId } from 'react';
import { Eye, EyeOff, AlertCircle, CheckCircle } from 'lucide-react';

interface InputFieldProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  success?: boolean;
  showPasswordToggle?: boolean;
  showPassword?: boolean;
  onTogglePassword?: () => void;
}

export const InputField = forwardRef<HTMLInputElement, InputFieldProps>(
  ({ 
    label, 
    error, 
    success, 
    showPasswordToggle, 
    showPassword, 
    onTogglePassword,
    className = '',
    id,
    ...props 
  }, ref) => {
    const generatedId = useId();
    const inputId = id || generatedId;
    const hasError = !!error;
    const hasSuccess = success && !hasError;

    return (
      <div className="space-y-2">
        <label htmlFor={inputId} className="block text-sm font-medium text-gray-700">
          {label}
          {props.required && <span className="text-red-500 ml-1">*</span>}
        </label>
        
        <div className="relative">
          <input
            id={inputId}
            ref={ref}
            className={`
              w-full px-4 py-3 border rounded-lg transition-all duration-200
              focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
              ${hasError 
                ? 'border-red-300 bg-red-50' 
                : hasSuccess 
                  ? 'border-green-300 bg-green-50'
                  : 'border-gray-300 bg-white hover:border-gray-400'
              }
              ${showPasswordToggle ? 'pr-12' : ''}
              ${className}
            `}
            {...props}
          />
          
          {/* Success/Error Icons */}
          {(hasError || hasSuccess) && !showPasswordToggle && (
            <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
              {hasError ? (
                <AlertCircle className="w-5 h-5 text-red-500" />
              ) : (
                <CheckCircle className="w-5 h-5 text-green-500" />
              )}
            </div>
          )}
          
          {/* Password Toggle */}
          {showPasswordToggle && (
            <button
              type="button"
              onClick={onTogglePassword}
              aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"}
              className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
            >
              {showPassword ? (
                <EyeOff className="w-5 h-5" />
              ) : (
                <Eye className="w-5 h-5" />
              )}
            </button>
          )}
        </div>
        
        {error && (
          <p className="text-sm text-red-600 flex items-center gap-1">
            <AlertCircle className="w-4 h-4" />
            {error}
          </p>
        )}
      </div>
    );
  }
);

InputField.displayName = 'InputField';
