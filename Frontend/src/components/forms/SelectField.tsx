import React, { forwardRef } from 'react';
import { AlertCircle, CheckCircle, ChevronDown } from 'lucide-react';

interface SelectOption {
  value: string;
  label: string;
}

interface SelectFieldProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  options: SelectOption[];
  error?: string;
  success?: boolean;
  placeholder?: string;
}

export const SelectField = forwardRef<HTMLSelectElement, SelectFieldProps>(
  ({ 
    label, 
    options, 
    error, 
    success, 
    placeholder = 'Selecione...',
    className = '',
    ...props 
  }, ref) => {
    const hasError = !!error;
    const hasSuccess = success && !hasError;

    return (
      <div className="space-y-2">
        <label className="block text-sm font-medium text-gray-700">
          {label}
          {props.required && <span className="text-red-500 ml-1">*</span>}
        </label>
        
        <div className="relative">
          <select
            ref={ref}
            className={`
              w-full px-4 py-3 border rounded-lg transition-all duration-200 appearance-none
              focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
              ${hasError 
                ? 'border-red-300 bg-red-50' 
                : hasSuccess 
                  ? 'border-green-300 bg-green-50'
                  : 'border-gray-300 bg-white hover:border-gray-400'
              }
              ${className}
            `}
            {...props}
          >
            <option value="">{placeholder}</option>
            {options.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
          
          {/* Dropdown Arrow */}
          <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400 pointer-events-none" />
          
          {/* Success/Error Icons */}
          {(hasError || hasSuccess) && (
            <div className="absolute right-10 top-1/2 transform -translate-y-1/2">
              {hasError ? (
                <AlertCircle className="w-5 h-5 text-red-500" />
              ) : (
                <CheckCircle className="w-5 h-5 text-green-500" />
              )}
            </div>
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

SelectField.displayName = 'SelectField';
