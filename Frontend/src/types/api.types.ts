export interface User {
  id: string;
  username: string;
  role: 1 | 2; // 1 = Admin, 2 = User (enum values)
  person?: any;
  createdAt: string;
  updatedAt: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface RegisterPersonData {
  nome: string;
  cpf: string;
  dataNascimento: string;
  email?: string;
  sexo?: "M" | "F" | "";
  naturalidade?: string;
  nacionalidade?: string;
  telefone?: string;
  celular?: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  person: RegisterPersonData;
}

export interface RegisterResponse {
  token: string;
  user: User;
}

export interface ContactInfoResponseDto {
  id: string;
  type: string;
  value: string;
  isPrimary: boolean;
}

export interface PersonResponseDto {
  id: string;
  nome: string;
  cpf: string;
  dataNascimento: string;
  email?: string;
  sexo?: "M" | "F" | "";
  naturalidade?: string;
  nacionalidade?: string;
  contacts: ContactInfoResponseDto[];
  createdByUserId?: string;
  updatedByUserId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePersonRequest {
  nome: string;
  cpf: string;
  dataNascimento: string;
  email?: string;
  sexo?: "M" | "F" | "";
  naturalidade?: string;
  nacionalidade?: string;
  telefone?: string;
  celular?: string;
}

export interface UpdatePersonRequest extends CreatePersonRequest {}

export interface ApiError {
  message: string;
  status: number;
}
