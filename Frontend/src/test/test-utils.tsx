import React, { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import { User } from '../types/api.types';

// Mock do AuthProvider para testes
const MockAuthProvider = ({ children }: { children: React.ReactNode }) => {
  return <>{children}</>;
};

// Mock do useAuth para testes
export const mockAuthContext = {
  user: null as User | null,
  token: null as string | null,
  login: vi.fn(),
  register: vi.fn(),
  logout: vi.fn(),
  isAuthenticated: false,
  isAdmin: false,
  loading: false,
};

// Wrapper para testes que precisam de roteamento e contexto de auth
const AllTheProviders = ({ children }: { children: React.ReactNode }) => {
  return (
    <BrowserRouter>
      <MockAuthProvider>
        {children}
      </MockAuthProvider>
    </BrowserRouter>
  );
};

// Wrapper apenas para roteamento
const RouterWrapper = ({ children }: { children: React.ReactNode }) => {
  return (
    <BrowserRouter>
      {children}
    </BrowserRouter>
  );
};

const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => render(ui, { wrapper: AllTheProviders, ...options });

const renderWithRouter = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => render(ui, { wrapper: RouterWrapper, ...options });

// Utilitário para criar um usuário mock
export const createMockUser = (overrides?: Partial<User>): User => ({
  id: '1',
  username: 'testuser',
  role: 2,
  createdAt: '2023-01-01T00:00:00Z',
  updatedAt: '2023-01-01T00:00:00Z',
  ...overrides,
});

// Utilitário para criar uma pessoa mock
export const createMockPerson = (overrides?: any) => ({
  id: '1',
  nome: 'João Silva',
  cpf: '123.456.789-00',
  dataNascimento: '1990-01-01',
  email: 'joao@example.com',
  sexo: 'M' as const,
  naturalidade: 'São Paulo',
  nacionalidade: 'Brasileira',
  contacts: [],
  createdAt: '2023-01-01T00:00:00Z',
  updatedAt: '2023-01-01T00:00:00Z',
  ...overrides,
});

// re-export everything
export * from '@testing-library/react';

// override render method
export { customRender as render, renderWithRouter };
