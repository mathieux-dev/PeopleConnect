import { describe, it, expect } from 'vitest';
import { LoadingSpinner } from '../LoadingSpinner';
import { render } from '../../../test/test-utils';

describe('LoadingSpinner', () => {
  it('deve renderizar o spinner', () => {
    render(<LoadingSpinner />);

    const spinner = document.querySelector('svg');
    expect(spinner).toBeInTheDocument();
    expect(spinner).toHaveClass('animate-spin');
  });

  it('deve aplicar tamanho pequeno quando especificado', () => {
    render(<LoadingSpinner size="sm" />);

    const spinner = document.querySelector('svg');
    expect(spinner).toHaveClass('w-4', 'h-4');
  });

  it('deve aplicar tamanho médio por padrão', () => {
    render(<LoadingSpinner />);

    const spinner = document.querySelector('svg');
    expect(spinner).toHaveClass('w-6', 'h-6');
  });  it('deve aplicar tamanho grande quando especificado', () => {
    render(<LoadingSpinner size="lg" />);
    
    const spinner = document.querySelector('svg');
    expect(spinner).toHaveClass('w-8', 'h-8');
  });

  it('deve aplicar classes customizadas', () => {
    const customClass = 'text-blue-500';
    render(<LoadingSpinner className={customClass} />);
    
    const spinner = document.querySelector('svg');
    expect(spinner).toHaveClass(customClass);
  });
});
