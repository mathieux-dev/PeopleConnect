import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { AuthProvider, useAuth } from '../useAuth';
import { createMockUser } from '../../test/test-utils';
import { authApi } from '../../services/api';

// Mock das APIs
vi.mock('../../services/api', () => ({
  authApi: {
    login: vi.fn(),
    register: vi.fn(),
  },
}));

vi.mock('react-hot-toast', () => ({
  default: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

// Typed mocks
const mockAuthApi = authApi as any;

// Mock do localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

describe('useAuth', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorageMock.getItem.mockReturnValue(null);
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <AuthProvider>{children}</AuthProvider>
  );

  it('deve inicializar com estado não autenticado', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.user).toBeNull();
    expect(result.current.token).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.isAdmin).toBe(false);
    expect(result.current.loading).toBe(false);
  });

  it('deve restaurar usuário do localStorage na inicialização', () => {
    const mockUser = createMockUser();
    const mockToken = 'mock-token';

    localStorageMock.getItem
      .mockReturnValueOnce(mockToken)
      .mockReturnValueOnce(JSON.stringify(mockUser));

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.user).toEqual(mockUser);
    expect(result.current.token).toBe(mockToken);
    expect(result.current.isAuthenticated).toBe(true);
  });

  it('deve fazer login com sucesso', async () => {
    const mockUser = createMockUser();
    const mockToken = 'mock-token';
    
    mockAuthApi.login.mockResolvedValue({
      user: mockUser,
      token: mockToken,
    });

    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.login('testuser', 'password');
    });

    expect(mockAuthApi.login).toHaveBeenCalledWith({
      username: 'testuser',
      password: 'password',
    });
    expect(result.current.user).toEqual(mockUser);
    expect(result.current.token).toBe(mockToken);
    expect(result.current.isAuthenticated).toBe(true);
    expect(localStorageMock.setItem).toHaveBeenCalledWith('token', mockToken);
    expect(localStorageMock.setItem).toHaveBeenCalledWith('user', JSON.stringify(mockUser));
  });

  it('deve identificar usuário admin corretamente', () => {
    const adminUser = createMockUser({ role: 1 });
    const mockToken = 'mock-token';

    localStorageMock.getItem
      .mockReturnValueOnce(mockToken)
      .mockReturnValueOnce(JSON.stringify(adminUser));

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.isAdmin).toBe(true);
  });

  it('deve identificar usuário comum corretamente', () => {
    const regularUser = createMockUser({ role: 2 });
    const mockToken = 'mock-token';

    localStorageMock.getItem
      .mockReturnValueOnce(mockToken)
      .mockReturnValueOnce(JSON.stringify(regularUser));

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.isAdmin).toBe(false);
  });

  it('deve registrar usuário com sucesso', async () => {
    const mockUser = createMockUser();
    const mockToken = 'mock-token';
    const registerData = {
      username: 'newuser',
      password: 'password123',
      person: {
        nome: 'João Silva',
        cpf: '123.456.789-00',
        dataNascimento: '1990-01-01',
      },
    };

    mockAuthApi.register.mockResolvedValue({
      user: mockUser,
      token: mockToken,
    });

    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.register(registerData);
    });

    expect(mockAuthApi.register).toHaveBeenCalledWith(registerData);
    expect(result.current.user).toEqual(mockUser);
    expect(result.current.token).toBe(mockToken);
    expect(result.current.isAuthenticated).toBe(true);
  });

  it('deve fazer logout corretamente', () => {
    const mockUser = createMockUser();
    const mockToken = 'mock-token';

    localStorageMock.getItem
      .mockReturnValueOnce(mockToken)
      .mockReturnValueOnce(JSON.stringify(mockUser));

    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => {
      result.current.logout();
    });

    expect(result.current.user).toBeNull();
    expect(result.current.token).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('token');
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('user');
  });

  it('deve tratar dados inválidos no localStorage', () => {
    // Simular JSON inválido que fará o parse falhar
    localStorageMock.getItem
      .mockReturnValueOnce('valid-token')
      .mockReturnValueOnce('invalid-json-that-will-crash');

    // Silenciar o console.error do JSON.parse
    const originalError = console.error;
    console.error = vi.fn();

    const { result } = renderHook(() => useAuth(), { wrapper });

    // Como o JSON.parse falha, o hook deve limpar os dados do localStorage
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('token');
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('user');
    
    // O usuário deve ser null pois o JSON.parse falhou
    expect(result.current.user).toBeNull();
    
    console.error = originalError;
  });

  it('deve lançar erro quando useAuth é usado fora do AuthProvider', () => {
    // Usando console.error para capturar erro do React
    const originalError = console.error;
    console.error = vi.fn();
    
    expect(() => {
      renderHook(() => useAuth());
    }).toThrow();
    
    console.error = originalError;
  });
});
