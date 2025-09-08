# PeopleConnect Frontend

Sistema de gerenciamento de pessoas desenvolvido em React/TypeScript com integração completa ao backend .NET.

## 🚀 Funcionalidades

### ✅ Autenticação e Autorização
- **Login** com validação de usuário e senha
- **Cadastro** completo com dados pessoais
- **Roles** (User/Admin) com permissões diferenciadas
- **Logout** seguro com limpeza de sessão
- **Proteção de rotas** baseada em autenticação

### 👥 Gerenciamento de Pessoas
- **Listagem** responsiva com cards informativos
- **Busca** por nome ou CPF em tempo real
- **Detalhes** completos em modal interativo
- **Contatos censurados** para usuários não logados
- **Exclusão** restrita a administradores

### 🎨 Interface e UX
- **Design moderno** com Tailwind CSS
- **Responsivo** para mobile, tablet e desktop
- **Animações suaves** e transições
- **Loading states** em todas as operações
- **Notificações toast** para feedback
- **Validação em tempo real** nos formulários

## 🛠 Tecnologias Utilizadas

- **React 18** com TypeScript
- **React Router Dom** para roteamento
- **React Hook Form** para formulários
- **Axios** para requisições HTTP
- **React Hot Toast** para notificações
- **Lucide React** para ícones
- **Tailwind CSS** para estilização
- **Vite** para build e desenvolvimento

## 📋 Pré-requisitos

- Node.js 16+ instalado
- Backend .NET rodando em `https://localhost:7000`

## 🚀 Como executar

1. **Instalar dependências:**
```bash
npm install
```

2. **Executar em desenvolvimento:**
```bash
npm run dev
```

3. **Acessar aplicação:**
```
http://localhost:3000
```

4. **Build para produção:**
```bash
npm run build
```

## 🔗 Integração com Backend

### Base URL
```
https://localhost:7000/api/v1
```

### Endpoints utilizados
- `POST /auth/login` - Autenticação
- `POST /auth/register` - Cadastro
- `GET /persons` - Listar pessoas
- `GET /persons/{id}` - Detalhes da pessoa
- `DELETE /persons/{id}` - Excluir pessoa (Admin)

### Headers automáticos
- `Authorization: Bearer {token}`
- `Content-Type: application/json`

## 📱 Funcionalidades por Página

### 🔐 Login (`/login`)
- Validação de usuário e senha
- Feedback visual de erros
- Redirecionamento automático
- Link para cadastro

### 📝 Cadastro (`/register`)
- Formulário em duas seções
- Validação de CPF e email
- Máscaras de formatação
- Validação de força da senha

### 👥 Listagem (`/persons`)
- Cards responsivos com informações
- Busca em tempo real
- Contatos censurados para não-logados
- Estatísticas no header
- Modal de detalhes completos

## 🔒 Segurança

- **Token JWT** armazenado no localStorage
- **Interceptors** para anexar headers automaticamente
- **Auto-logout** em caso de token expirado (401)
- **Validação de permissões** antes de ações sensíveis
- **Proteção de rotas** baseada em autenticação

## 📱 Responsividade

- **Mobile-first** design
- **Breakpoints:** sm (640px), md (768px), lg (1024px)
- **Grid adaptativo:** 1→2→3+ colunas conforme tela
- **Navegação otimizada** para touch

## 🎯 Validações Implementadas

### Formulários
- **CPF:** Algoritmo completo de validação
- **Email:** Regex pattern matching
- **Senha:** Força com critérios específicos
- **Username:** 3-20 caracteres alfanuméricos
- **Datas:** Formato DD/MM/AAAA

### Formatação automática
- **CPF:** 000.000.000-00
- **Telefone:** (00) 00000-0000
- **Data:** DD/MM/AAAA

## 🚨 Tratamento de Erros

- **401:** Auto-logout + redirect login
- **403:** Mensagem de permissão negada
- **404:** Recurso não encontrado
- **409:** Dados duplicados
- **400:** Dados inválidos
- **Network:** Erro de conexão

## 📊 Estados da Aplicação

### Loading States
- Spinner durante autenticação
- Loading em operações assíncronas
- Skeleton screens para listas

### Empty States
- Nenhuma pessoa encontrada
- Resultados de busca vazios
- Estados de erro amigáveis

## 🎨 Design System

### Cores principais
- **Primária:** Blue (600/700)
- **Sucesso:** Green (500/600)
- **Erro:** Red (500/600)
- **Neutros:** Gray (50-900)

### Componentes reutilizáveis
- `InputField` - Campo de entrada com validação
- `SelectField` - Seleção com opções
- `LoadingSpinner` - Indicador de carregamento
- `PersonCard` - Card de pessoa
- `PersonDetailsModal` - Modal de detalhes

## 🔄 Fluxo de Autenticação

1. **Usuário acessa** aplicação
2. **Verifica token** no localStorage
3. **Se válido:** Redireciona para `/persons`
4. **Se inválido:** Redireciona para `/login`
5. **Após login:** Armazena token e dados do usuário
6. **Interceptor** anexa token em todas as requisições
7. **Em erro 401:** Auto-logout e redirect

## 📈 Performance

- **Code splitting** automático pelo Vite
- **Lazy loading** de componentes
- **Otimização de imagens** com lazy loading
- **Debounce** na busca em tempo real
- **Memoização** de componentes pesados

## 🧪 Estrutura do Projeto

```
src/
├── components/
│   ├── common/          # Componentes reutilizáveis
│   ├── forms/           # Componentes de formulário
│   └── modals/          # Modais da aplicação
├── pages/
│   ├── Login/           # Página de login
│   ├── Register/        # Página de cadastro
│   └── Persons/         # Página de listagem
├── services/
│   └── api.ts           # Configuração do Axios
├── hooks/
│   └── useAuth.ts       # Context de autenticação
├── types/
│   └── api.types.ts     # Tipagens TypeScript
├── utils/
│   ├── validation.ts    # Funções de validação
│   └── formatters.ts    # Formatação de dados
└── App.tsx              # Componente principal
```

## 🎯 Próximos Passos

- [ ] Implementar CRUD completo de pessoas
- [ ] Adicionar filtros avançados
- [ ] Implementar paginação
- [ ] Adicionar testes unitários
- [ ] Implementar PWA
- [ ] Adicionar modo escuro
- [ ] Implementar exportação de dados

---

**Desenvolvido com ❤️ usando React + TypeScript + Tailwind CSS**
