import axios, { AxiosResponse, AxiosError } from 'axios';
import toast from 'react-hot-toast';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  PersonResponseDto,
  CreatePersonRequest,
  UpdatePersonRequest,
  ApiError
} from '../types/api.types';

const BASE_URL = 'https://peopleconnect-api.onrender.com/api/v1';

// Create axios instance
export const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
api.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: AxiosError) => {
    const apiError: ApiError = {
      message: 'Erro desconhecido',
      status: error.response?.status || 500,
    };

    if (error.response?.status === 401) {
      // Auto logout on 401
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
      apiError.message = 'Sessão expirada. Faça login novamente.';
    } else if (error.response?.status === 403) {
      apiError.message = 'Você não tem permissão para esta ação.';
    } else if (error.response?.status === 404) {
      apiError.message = 'Recurso não encontrado.';
    } else if (error.response?.status === 409) {
      apiError.message = 'Dados já existem no sistema.';
    } else if (error.response?.status === 400) {
      apiError.message = 'Dados inválidos fornecidos.';
    } else if (error.code === 'NETWORK_ERROR' || !error.response) {
      apiError.message = 'Erro de conexão. Verifique sua internet.';
    }

    toast.error(apiError.message);
    return Promise.reject(apiError);
  }
);

// Auth API
export const authApi = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<RegisterResponse> => {
    const response = await api.post<RegisterResponse>('/auth/register', data);
    return response.data;
  },
};

// Persons API
export const personsApi = {
  getAll: async (): Promise<PersonResponseDto[]> => {
    const response = await api.get<PersonResponseDto[]>('/persons');
    return response.data;
  },

  getById: async (id: string): Promise<PersonResponseDto> => {
    const response = await api.get<PersonResponseDto>(`/persons/${id}`);
    return response.data;
  },

  create: async (data: CreatePersonRequest): Promise<PersonResponseDto> => {
    const response = await api.post<PersonResponseDto>('/persons', data);
    return response.data;
  },

  update: async (id: string, data: UpdatePersonRequest): Promise<PersonResponseDto> => {
    const response = await api.put<PersonResponseDto>(`/persons/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/persons/${id}`);
  },
};
