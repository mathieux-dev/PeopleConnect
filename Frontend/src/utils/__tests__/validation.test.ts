import { describe, it, expect } from 'vitest';
import {
  validateCPF,
  validateEmail,
  validatePassword,
  validateUsername
} from '../validation';

describe('validation', () => {
  describe('validateCPF', () => {
    it('deve validar CPFs válidos', () => {
      expect(validateCPF('11144477735')).toBe(true);
      expect(validateCPF('123.456.789-09')).toBe(true);
    });

    it('deve rejeitar CPFs inválidos', () => {
      expect(validateCPF('12345678901')).toBe(false);
      expect(validateCPF('11111111111')).toBe(false);
      expect(validateCPF('000.000.000-00')).toBe(false);
    });

    it('deve rejeitar CPFs com formato incorreto', () => {
      expect(validateCPF('123456789')).toBe(false); // Menos de 11 dígitos
      expect(validateCPF('123456789012')).toBe(false); // Mais de 11 dígitos
      expect(validateCPF('')).toBe(false); // Vazio
      expect(validateCPF('abc.def.ghi-jk')).toBe(false); // Não numérico
    });
  });

  describe('validateEmail', () => {
    it('deve validar emails válidos', () => {
      expect(validateEmail('user@example.com')).toBe(true);
      expect(validateEmail('test.email+tag@domain.co')).toBe(true);
      expect(validateEmail('user123@subdomain.example.org')).toBe(true);
    });

    it('deve rejeitar emails inválidos', () => {
      expect(validateEmail('invalid-email')).toBe(false);
      expect(validateEmail('@domain.com')).toBe(false);
      expect(validateEmail('user@')).toBe(false);
      expect(validateEmail('user@.com')).toBe(false);
      expect(validateEmail('')).toBe(false);
      expect(validateEmail('user @domain.com')).toBe(false); // Espaço
    });
  });

  describe('validatePassword', () => {
    it('deve validar senha forte', () => {
      const result = validatePassword('MyStrongPass123');
      expect(result.isValid).toBe(true);
      expect(result.strength).toBe('strong');
      expect(result.errors).toHaveLength(0);
    });

    it('deve identificar senha média', () => {
      const result = validatePassword('password123'); // Falta maiúscula
      expect(result.isValid).toBe(false);
      expect(result.strength).toBe('medium');
      expect(result.errors).toContain('Uma letra maiúscula');
    });

    it('deve identificar senha fraca', () => {
      const result = validatePassword('pass');
      expect(result.isValid).toBe(false);
      expect(result.strength).toBe('weak');
      expect(result.errors).toContain('Mínimo 6 caracteres');
      expect(result.errors).toContain('Uma letra maiúscula');
      expect(result.errors).toContain('Um número');
    });

    it('deve verificar todos os critérios de validação', () => {
      const weakPassword = validatePassword('123');
      expect(weakPassword.errors).toContain('Mínimo 6 caracteres');
      expect(weakPassword.errors).toContain('Uma letra maiúscula');
      expect(weakPassword.errors).toContain('Uma letra minúscula');

      const noUppercase = validatePassword('password123');
      expect(noUppercase.errors).toContain('Uma letra maiúscula');

      const noLowercase = validatePassword('PASSWORD123');
      expect(noLowercase.errors).toContain('Uma letra minúscula');

      const noNumber = validatePassword('Password');
      expect(noNumber.errors).toContain('Um número');
    });
  });

  describe('validateUsername', () => {
    it('deve validar usernames válidos', () => {
      expect(validateUsername('user123')).toBe(true);
      expect(validateUsername('User_Name')).toBe(true);
      expect(validateUsername('abc')).toBe(true); // Mínimo 3 caracteres
      expect(validateUsername('a'.repeat(20))).toBe(true); // Máximo 20 caracteres
    });

    it('deve rejeitar usernames inválidos', () => {
      expect(validateUsername('ab')).toBe(false); // Muito curto
      expect(validateUsername('a'.repeat(21))).toBe(false); // Muito longo
      expect(validateUsername('user-name')).toBe(false); // Hífen não permitido
      expect(validateUsername('user name')).toBe(false); // Espaço não permitido
      expect(validateUsername('user@name')).toBe(false); // @ não permitido
      expect(validateUsername('')).toBe(false); // Vazio
      expect(validateUsername('user.name')).toBe(false); // Ponto não permitido
    });

    it('deve permitir apenas caracteres alfanuméricos e underscore', () => {
      expect(validateUsername('user_123')).toBe(true);
      expect(validateUsername('User123')).toBe(true);
      expect(validateUsername('123user')).toBe(true);
      expect(validateUsername('_user')).toBe(true);
      expect(validateUsername('user_')).toBe(true);
    });
  });
});
