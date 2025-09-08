# Frontend - Testes

Este diretório contém todos os testes do frontend da aplicação PeopleConnect.

## Estrutura de Testes

```
src/
├── test/
│   ├── setup.ts              # Configuração global dos testes
│   └── test-utils.tsx        # Utilitários e helpers para testes
├── components/
│   └── **/__tests__/         # Testes dos componentes
├── pages/
│   └── **/__tests__/         # Testes das páginas
├── hooks/
│   └── __tests__/            # Testes dos hooks
├── utils/
│   └── __tests__/            # Testes dos utilitários
└── App.test.tsx              # Testes de integração do App
```

## Tipos de Testes

### 1. Testes Unitários
- **Componentes**: Testam componentes isoladamente
- **Hooks**: Testam hooks customizados
- **Utilitários**: Testam funções de formatação e validação

### 2. Testes de Integração
- **Páginas**: Testam interação entre componentes
- **App**: Testa roteamento e fluxos principais

## Scripts Disponíveis

```bash
# Executar todos os testes
npm test

# Executar testes em modo watch
npm run test:watch

# Executar testes com interface gráfica
npm run test:ui

# Executar testes com relatório de cobertura
npm run test:coverage
```

## Ferramentas Utilizadas

- **Vitest**: Framework de teste rápido e moderno
- **Testing Library**: Biblioteca para testes focados no usuário
- **jsdom**: Ambiente DOM para testes
- **User Event**: Simulação de eventos de usuário

## Convenções

### Nomenclatura
- Arquivos de teste: `*.test.tsx` ou `*.test.ts`
- Diretório: `__tests__/` dentro de cada módulo

### Estrutura dos Testes
```typescript
describe('ComponentName', () => {
  beforeEach(() => {
    // Setup antes de cada teste
  });

  it('deve fazer algo específico', () => {
    // Arrange
    // Act
    // Assert
  });
});
```

### Mocks
- Use `vi.mock()` para mockar módulos
- Use `vi.fn()` para mockar funções
- Limpe mocks com `vi.clearAllMocks()` no `beforeEach`

## Cobertura de Testes

A cobertura está configurada para gerar relatórios em:
- Console (texto)
- JSON (`coverage/coverage.json`)
- HTML (`coverage/index.html`)

### Exclusões
- `node_modules/`
- Arquivos de configuração
- Arquivos de definição de tipos
- Diretório de testes

## Exemplos de Uso

### Testando Componente
```typescript
import { render, screen } from '../../../test/test-utils';
import { MyComponent } from '../MyComponent';

describe('MyComponent', () => {
  it('deve renderizar com props corretas', () => {
    render(<MyComponent title="Teste" />);
    expect(screen.getByText('Teste')).toBeInTheDocument();
  });
});
```

### Testando Hook
```typescript
import { renderHook, act } from '@testing-library/react';
import { useMyHook } from '../useMyHook';

describe('useMyHook', () => {
  it('deve retornar valor inicial', () => {
    const { result } = renderHook(() => useMyHook());
    expect(result.current.value).toBe(initialValue);
  });
});
```

### Testando Interação do Usuário
```typescript
import userEvent from '@testing-library/user-event';

it('deve chamar função ao clicar', async () => {
  const user = userEvent.setup();
  const mockFn = vi.fn();
  
  render(<Button onClick={mockFn}>Clique</Button>);
  await user.click(screen.getByRole('button'));
  
  expect(mockFn).toHaveBeenCalledTimes(1);
});
```

## Boas Práticas

1. **Teste comportamento, não implementação**
2. **Use queries semânticas** (`getByRole`, `getByLabelText`)
3. **Simule interações do usuário** com `userEvent`
4. **Mantenha testes independentes** e isolados
5. **Use mocks apenas quando necessário**
6. **Escreva testes legíveis** com descrições claras
7. **Teste casos de erro** além dos casos de sucesso

## Debugging

Para debuggar testes:
```typescript
import { screen } from '@testing-library/react';

// Ver HTML atual
screen.debug();

// Ver elemento específico
screen.debug(screen.getByRole('button'));
```

## Continuous Integration

Os testes são executados automaticamente em:
- Pull requests
- Push para branch principal
- Builds de produção

Certifique-se de que todos os testes passem antes de fazer merge!
