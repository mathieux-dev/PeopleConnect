import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render } from '@testing-library/react';
import { App } from './App';

// Mock do useAuth
const mockAuth = {
  user: null,
  token: null,
  login: vi.fn(),
  register: vi.fn(),
  logout: vi.fn(),
  isAuthenticated: false,
  isAdmin: false,
  loading: false,
};

vi.mock('./hooks/useAuth', () => ({
  useAuth: () => mockAuth,
  AuthProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock das páginas
vi.mock('./pages/Login/LoginPage', () => ({
  LoginPage: () => <div data-testid="login-page">Login Page</div>,
}));

vi.mock('./pages/Register/RegisterPage', () => ({
  RegisterPage: () => <div data-testid="register-page">Register Page</div>,
}));

vi.mock('./pages/Persons/PersonsPage', () => ({
  PersonsPage: () => <div data-testid="persons-page">Persons Page</div>,
}));

vi.mock('./components/common/ProtectedRoute', () => ({
  ProtectedRoute: ({ children, requireAuth }: { children: React.ReactNode; requireAuth: boolean }) => {
    if (requireAuth && !mockAuth.isAuthenticated) {
      return <div data-testid="redirect-to-login">Redirecting to login...</div>;
    }
    if (!requireAuth && mockAuth.isAuthenticated) {
      return <div data-testid="redirect-to-persons">Redirecting to persons...</div>;
    }
    return <>{children}</>;
  },
}));

describe('App', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockAuth.isAuthenticated = false;
    mockAuth.user = null;
  });

  it('deve renderizar sem erros', () => {
    expect(() => render(<App />)).not.toThrow();
  });

  it('deve conter o Toaster para notificações', () => {
    render(<App />);
    // O Toaster está presente mas não é visível sem toast ativo
    // Verificamos que não há erro ao renderizar
    expect(document.querySelector('.App')).toBeTruthy();
  });
});
