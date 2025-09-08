import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, fireEvent } from '@testing-library/react';
import { PersonCard } from '../PersonCard';
import { render, createMockPerson } from '../../../test/test-utils';

// Mock do useAuth
const mockUseAuth = vi.fn();

vi.mock('../../../hooks/useAuth', () => ({
  useAuth: () => mockUseAuth(),
}));

describe('PersonCard', () => {
  const mockPerson = createMockPerson();
  const mockOnViewDetails = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    // Mock padrão para useAuth
    mockUseAuth.mockReturnValue({
      isAuthenticated: true,
      isAdmin: false,
    });
  });

  it('deve renderizar as informações básicas da pessoa', () => {
    render(
      <PersonCard person={mockPerson} onViewDetails={mockOnViewDetails} />
    );

    expect(screen.getByText(mockPerson.nome)).toBeInTheDocument();
    expect(screen.getByText(`CPF: ${mockPerson.cpf}`)).toBeInTheDocument();
    expect(screen.getByText('Masculino')).toBeInTheDocument();
  });

  it('deve exibir "Feminino" para pessoa do sexo F', () => {
    const femininePerson = createMockPerson({ sexo: 'F' });
    
    render(
      <PersonCard person={femininePerson} onViewDetails={mockOnViewDetails} />
    );

    expect(screen.getByText('Feminino')).toBeInTheDocument();
  });

  it('deve exibir "Não Informado" quando sexo não é informado', () => {
    const unknownGenderPerson = createMockPerson({ sexo: '' });
    
    render(
      <PersonCard person={unknownGenderPerson} onViewDetails={mockOnViewDetails} />
    );

    expect(screen.getByText('Não Informado')).toBeInTheDocument();
  });

  it('deve exibir naturalidade quando fornecida', () => {
    render(
      <PersonCard person={mockPerson} onViewDetails={mockOnViewDetails} />
    );

    expect(screen.getByText(/São Paulo/)).toBeInTheDocument();
  });

  it('deve exibir nacionalidade quando fornecida', () => {
    render(
      <PersonCard person={mockPerson} onViewDetails={mockOnViewDetails} />
    );

    expect(screen.getByText(/Brasileira/)).toBeInTheDocument();
  });

  it('deve chamar onViewDetails quando botão "Ver Detalhes" é clicado', () => {
    render(
      <PersonCard person={mockPerson} onViewDetails={mockOnViewDetails} />
    );

    const viewButton = screen.getByRole('button', { name: /ver detalhes/i });
    fireEvent.click(viewButton);

    expect(mockOnViewDetails).toHaveBeenCalledWith(mockPerson);
    expect(mockOnViewDetails).toHaveBeenCalledTimes(1);
  });

  it('deve aplicar as classes CSS corretas para cada sexo', () => {
    const { rerender } = render(
      <PersonCard person={mockPerson} onViewDetails={mockOnViewDetails} />
    );

    // Masculino
    expect(screen.getByText('Masculino')).toHaveClass('bg-blue-100', 'text-blue-800');

    // Feminino
    const femininePerson = createMockPerson({ sexo: 'F' });
    rerender(
      <PersonCard person={femininePerson} onViewDetails={mockOnViewDetails} />
    );
    expect(screen.getByText('Feminino')).toHaveClass('bg-pink-100', 'text-pink-800');

    // Não informado
    const unknownPerson = createMockPerson({ sexo: '' });
    rerender(
      <PersonCard person={unknownPerson} onViewDetails={mockOnViewDetails} />
    );
    expect(screen.getByText('Não Informado')).toHaveClass('bg-gray-100', 'text-gray-800');
  });
});
