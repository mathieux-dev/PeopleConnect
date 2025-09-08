import { describe, it, expect } from 'vitest';
import {
  formatCPF,
  formatPhone,
  formatDate,
  formatDateForAPI,
  formatDateFromAPI,
  censorContact,
  getContactIcon
} from '../formatters';

describe('formatters', () => {
  describe('formatCPF', () => {
    it('deve formatar CPF corretamente', () => {
      expect(formatCPF('12345678901')).toBe('123.456.789-01');
      expect(formatCPF('123456789')).toBe('123.456.789');
      expect(formatCPF('123456')).toBe('123.456');
      expect(formatCPF('123')).toBe('123');
    });

    it('deve remover caracteres não numéricos', () => {
      expect(formatCPF('123.456.789-01')).toBe('123.456.789-01');
      expect(formatCPF('123abc456def789ghi01')).toBe('123.456.789-01');
    });

    it('deve retornar valor original se exceder 11 dígitos', () => {
      expect(formatCPF('123456789012')).toBe('123456789012');
    });
  });

  describe('formatPhone', () => {
    it('deve formatar telefone fixo corretamente', () => {
      expect(formatPhone('1122334455')).toBe('(11) 2233-4455');
    });

    it('deve formatar celular corretamente', () => {
      expect(formatPhone('11987654321')).toBe('(11) 98765-4321');
    });

    it('deve formatar números parciais', () => {
      expect(formatPhone('11987')).toBe('(11) 987');
      expect(formatPhone('119876')).toBe('(11) 9876');
    });

    it('deve retornar valor original se exceder 11 dígitos', () => {
      expect(formatPhone('119876543212')).toBe('119876543212');
    });
  });

  describe('formatDate', () => {
    it('deve formatar data corretamente', () => {
      expect(formatDate('01012023')).toBe('01/01/2023');
      expect(formatDate('0101')).toBe('01/01');
      expect(formatDate('01')).toBe('01');
    });

    it('deve remover caracteres não numéricos', () => {
      expect(formatDate('01/01/2023')).toBe('01/01/2023');
      expect(formatDate('01a01b2023')).toBe('01/01/2023');
    });
  });

  describe('formatDateForAPI', () => {
    it('deve converter DD/MM/YYYY para YYYY-MM-DD', () => {
      expect(formatDateForAPI('01/12/2023')).toBe('2023-12-01');
      expect(formatDateForAPI('15/06/1990')).toBe('1990-06-15');
    });

    it('deve adicionar zeros à esquerda quando necessário', () => {
      expect(formatDateForAPI('1/2/2023')).toBe('2023-02-01');
    });
  });

  describe('formatDateFromAPI', () => {
    it('deve converter YYYY-MM-DD para DD/MM/YYYY', () => {
      expect(formatDateFromAPI('2023-12-01')).toBe('01/12/2023');
      expect(formatDateFromAPI('1990-06-15')).toBe('15/06/1990');
    });

    it('deve lidar com datas com timestamp', () => {
      expect(formatDateFromAPI('2023-12-01T10:30:00Z')).toBe('01/12/2023');
    });
  });

  describe('censorContact', () => {
    it('deve censurar email corretamente', () => {
      expect(censorContact('joao@example.com', 'email')).toBe('j**o@*******.com');
      expect(censorContact('a@test.com', 'email')).toBe('a@****.com');
    });

    it('deve censurar telefone corretamente', () => {
      expect(censorContact('11987654321', 'telefone')).toBe('(11) 9****-****');
      expect(censorContact('1122334455', 'whatsapp')).toBe('(11) 2****-****');
    });

    it('deve censurar outros tipos de contato genericamente', () => {
      expect(censorContact('linkedin.com/joao', 'linkedin')).toBe('l***************o');
      expect(censorContact('abc', 'other')).toBe('***');
      expect(censorContact('ab', 'other')).toBe('**');
    });
  });

  describe('getContactIcon', () => {
    it('deve retornar ícone correto para cada tipo de contato', () => {
      expect(getContactIcon('email')).toBe('📧');
      expect(getContactIcon('telefone')).toBe('📱');
      expect(getContactIcon('whatsapp')).toBe('💬');
      expect(getContactIcon('linkedin')).toBe('💼');
      expect(getContactIcon('instagram')).toBe('📷');
      expect(getContactIcon('facebook')).toBe('👥');
    });

    it('deve retornar ícone padrão para tipos desconhecidos', () => {
      expect(getContactIcon('unknown')).toBe('📞');
      expect(getContactIcon('')).toBe('📞');
    });

    it('deve ser case insensitive', () => {
      expect(getContactIcon('EMAIL')).toBe('📧');
      expect(getContactIcon('WhatsApp')).toBe('💬');
      expect(getContactIcon('LINKEDIN')).toBe('💼');
    });
  });
});
