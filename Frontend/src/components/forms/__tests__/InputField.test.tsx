import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { InputField } from '../InputField';
import { render } from '../../../test/test-utils';

describe('InputField', () => {
  it('deve renderizar com label', () => {
    render(<InputField label="Nome de usuário" />);
    
    expect(screen.getByLabelText(/nome de usuário/i)).toBeInTheDocument();
    expect(screen.getByText('Nome de usuário')).toBeInTheDocument();
  });

  it('deve mostrar asterisco para campos obrigatórios', () => {
    render(<InputField label="Email" required />);
    
    expect(screen.getByText('*')).toBeInTheDocument();
  });

  it('deve mostrar mensagem de erro', () => {
    render(<InputField label="Email" error="Email é obrigatório" />);
    
    expect(screen.getByText('Email é obrigatório')).toBeInTheDocument();
    expect(screen.getByText('Email é obrigatório')).toHaveClass('text-red-600');
  });

  it('deve aplicar estilos de erro', () => {
    render(<InputField label="Email" error="Email é obrigatório" />);
    
    const input = screen.getByLabelText(/email/i);
    expect(input).toHaveClass('border-red-300', 'bg-red-50');
  });

  it('deve aplicar estilos de sucesso', () => {
    render(<InputField label="Email" success />);
    
    const input = screen.getByLabelText(/email/i);
    expect(input).toHaveClass('border-green-300', 'bg-green-50');
  });

  it('deve renderizar toggle de senha quando especificado', () => {
    render(
      <InputField 
        label="Senha" 
        type="password" 
        showPasswordToggle 
        showPassword={false}
        onTogglePassword={vi.fn()}
      />
    );
    
    expect(screen.getByRole('button', { name: /mostrar senha/i })).toBeInTheDocument();
  });

  it('deve alternar ícone do toggle de senha', () => {
    const { rerender } = render(
      <InputField 
        label="Senha" 
        type="password" 
        showPasswordToggle 
        showPassword={false}
        onTogglePassword={vi.fn()}
      />
    );
    
    expect(screen.getByRole('button', { name: /mostrar senha/i })).toBeInTheDocument();
    
    rerender(
      <InputField 
        label="Senha" 
        type="text" 
        showPasswordToggle 
        showPassword={true}
        onTogglePassword={vi.fn()}
      />
    );
    
    expect(screen.getByRole('button', { name: /ocultar senha/i })).toBeInTheDocument();
  });

  it('deve chamar onTogglePassword quando toggle é clicado', async () => {
    const user = userEvent.setup();
    const mockToggle = vi.fn();
    
    render(
      <InputField 
        label="Senha" 
        type="password" 
        showPasswordToggle 
        showPassword={false}
        onTogglePassword={mockToggle}
      />
    );
    
    const toggleButton = screen.getByRole('button', { name: /mostrar senha/i });
    await user.click(toggleButton);
    
    expect(mockToggle).toHaveBeenCalledTimes(1);
  });

  it('deve aceitar props HTML padrão', () => {
    render(
      <InputField 
        label="Email" 
        placeholder="Digite seu email"
        disabled
        value="test@example.com"
        onChange={vi.fn()}
      />
    );
    
    const input = screen.getByLabelText(/email/i);
    expect(input).toHaveAttribute('placeholder', 'Digite seu email');
    expect(input).toBeDisabled();
    expect(input).toHaveValue('test@example.com');
  });

  it('deve permitir classes CSS customizadas', () => {
    render(<InputField label="Nome" className="custom-class" />);
    
    const input = screen.getByLabelText(/nome/i);
    expect(input).toHaveClass('custom-class');
  });

  it('deve priorizar erro sobre sucesso', () => {
    render(<InputField label="Email" error="Email inválido" success />);
    
    const input = screen.getByLabelText(/email/i);
    expect(input).toHaveClass('border-red-300', 'bg-red-50');
    expect(input).not.toHaveClass('border-green-300', 'bg-green-50');
  });

  it('deve permitir referência ao input', () => {
    const ref = React.createRef<HTMLInputElement>();
    render(<InputField ref={ref} label="Nome" />);
    
    expect(ref.current).toBeInstanceOf(HTMLInputElement);
  });
});
