# PeopleConnect

Sistema fullstack para gerenciamento de pessoas e contatos.

## 🏗️ Arquitetura

- **Backend**: .NET 8 Web API com Clean Architecture
- **Frontend**: Vite + React/Vue com Tailwind CSS
- **Banco**: PostgreSQL 16
- **Proxy**: Nginx (produção)

## 🚀 Deploy

### Desenvolvimento

```powershell
# Subir ambiente completo para desenvolvimento
.\deploy.ps1

# Ou usando docker-compose diretamente
docker-compose up -d
```

**Serviços disponíveis:**
- Frontend: http://localhost:3000
- Backend: http://localhost:5001
- PgAdmin: http://localhost:8080 (admin@peopleconnect.com / admin123)

### Produção

```powershell
# 1. Configure as variáveis de ambiente
cp .env.example .env
# Edite o arquivo .env com suas configurações

# 2. Deploy de produção
.\deploy.ps1 -Production

# Ou usando docker-compose diretamente
docker-compose -f docker-compose.prod.yml up -d
```

**Serviços disponíveis:**
- Aplicação completa: http://localhost

### Comandos Úteis

```powershell
# Parar todos os containers
.\deploy.ps1 -Stop

# Ver logs do desenvolvimento
.\deploy.ps1 -Logs

# Ver logs da produção
.\deploy.ps1 -Logs -Production

# Rebuild das imagens
docker-compose build --no-cache
```

## 🧪 Testes

### Testes Unitários
```powershell
cd Backend
dotnet test --configuration Release --logger trx --collect:"XPlat Code Coverage"
```

### Testes de Integração
```powershell
cd Backend/Tests
docker-compose -f docker-compose.test.yml up -d
dotnet test ../Application/PeopleConnect.Application.Tests/ --settings ../coverlet.integration.runsettings
docker-compose -f docker-compose.test.yml down
```

## 📁 Estrutura do Projeto

```
PeopleConnect/
├── Backend/                    # API .NET 8
│   ├── Domain/                # Entidades e regras de negócio
│   ├── Application/           # Use cases e DTOs
│   ├── Infrastructure/        # Acesso a dados e integrações
│   ├── Presentation/          # Controllers e configurações
│   ├── Tests/                 # Testes de integração
│   └── Dockerfile
├── Frontend/                   # SPA com Vite
│   ├── src/
│   ├── nginx.conf
│   └── Dockerfile
├── docker-compose.yml          # Desenvolvimento
├── docker-compose.prod.yml     # Produção
├── nginx.prod.conf            # Configuração Nginx produção
└── deploy.ps1                 # Script de deploy
```

## ⚙️ Configuração

### Variáveis de Ambiente (.env)

```env
# Database
POSTGRES_DB=peopleconnect
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password

# Application
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET=your_jwt_secret_key

# URLs
API_BASE_URL=http://localhost/api
FRONTEND_URL=http://localhost
```

## 📊 Monitoramento

### Health Checks
- Backend: http://localhost:5001/health (dev) ou http://localhost/api/health (prod)
- Frontend: Incluído no nginx
- Database: Configurado no docker-compose

### Logs
```powershell
# Ver logs de todos os serviços
docker-compose logs -f

# Ver logs de um serviço específico
docker-compose logs -f backend
```

## 🔧 Desenvolvimento

### Pré-requisitos
- Docker Desktop
- .NET 8 SDK (opcional, para desenvolvimento local)
- Node.js 18+ (opcional, para desenvolvimento local)

### Setup Local
1. Clone o repositório
2. Execute `.\deploy.ps1` para ambiente de desenvolvimento
3. Acesse http://localhost:3000

## 📝 Notas de Deploy

- O ambiente de produção usa Nginx como reverse proxy
- Rate limiting configurado para APIs
- Health checks implementados em todos os serviços
- Volumes persistentes para dados do PostgreSQL
- SSL/TLS pode ser configurado editando nginx.prod.conf
