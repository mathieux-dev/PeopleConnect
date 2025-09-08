# Testes de Integração - PeopleConnect

Este projeto contém testes de integração completos para a API PeopleConnect, utilizando Docker para isolamento e containers de teste.

## 🏗️ Arquitetura dos Testes

### Infraestrutura
- **TestContainers**: Utiliza PostgreSQL em container Docker para isolamento dos testes
- **WebApplicationFactory**: Factory personalizada para configurar a aplicação de teste
- **Database Seeder**: Popula o banco com dados de teste controlados

### Estrutura dos Testes
```
PeopleConnect.IntegrationTests/
├── Controllers/                    # Testes dos endpoints da API
│   ├── AuthControllerTests.cs     # Autenticação e registro
│   ├── PersonsControllerTests.cs  # CRUD de pessoas
│   ├── UsersControllerTests.cs    # Gestão de usuários
│   └── HealthControllerTests.cs   # Health check
├── Scenarios/                     # Testes de fluxo completo
│   └── EndToEndWorkflowTests.cs   # Cenários E2E
├── Infrastructure/                # Configuração dos testes
│   ├── IntegrationTestWebAppFactory.cs
│   ├── IntegrationTestBase.cs
│   └── DatabaseSeeder.cs
└── Models/                        # DTOs para testes
    └── ResponseDtos.cs
```

## 🚀 Executando os Testes

### Pré-requisitos
- Docker Desktop instalado e rodando
- .NET 8 SDK
- PowerShell (para scripts de automação)

### Opções de Execução

#### 1. Workflow Completo (Recomendado)
```powershell
.\run-integration-tests.ps1 -All
```
Inicia containers, executa testes e limpa infraestrutura.

#### 2. Com Cobertura de Código
```powershell
.\run-integration-tests.ps1 -All -WithCoverage
```

#### 3. Filtrar Testes Específicos
```powershell
.\run-integration-tests.ps1 -All -Filter "*Auth*"
```

#### 4. Controle Manual da Infraestrutura
```powershell
# Iniciar apenas a infraestrutura
.\run-integration-tests.ps1 -StartContainers

# Executar apenas os testes (infraestrutura deve estar rodando)
.\run-integration-tests.ps1 -RunTests

# Parar a infraestrutura
.\run-integration-tests.ps1 -StopContainers
```

#### 5. Usando dotnet diretamente
```powershell
# Executar todos os testes
dotnet test Tests\PeopleConnect.IntegrationTests\

# Com verbosidade detalhada
dotnet test Tests\PeopleConnect.IntegrationTests\ --verbosity detailed

# Com filtro
dotnet test Tests\PeopleConnect.IntegrationTests\ --filter "FullyQualifiedName~Auth"
```

## 📋 Cenários de Teste Cobertos

### Autenticação (AuthControllerTests)
- ✅ Login com credenciais válidas
- ✅ Login com credenciais inválidas  
- ✅ Login com usuário inexistente
- ✅ Registro de novo usuário
- ✅ Registro com email duplicado
- ✅ Registro com CPF duplicado
- ✅ Validações de entrada (email, senha)

### Gestão de Pessoas (PersonsControllerTests)
- ✅ Listar todas as pessoas
- ✅ Buscar pessoa por ID
- ✅ Criar nova pessoa (Admin)
- ✅ Atualizar pessoa (Admin)
- ✅ Deletar pessoa (Admin)
- ✅ Controle de permissões (User vs Admin)
- ✅ Validações de negócio (CPF duplicado)

### Gestão de Usuários (UsersControllerTests)
- ✅ Listar usuários (Admin only)
- ✅ Buscar usuário por ID (Admin only)
- ✅ Controle de acesso baseado em roles

### Health Check (HealthControllerTests)
- ✅ Endpoint de saúde da aplicação

### Fluxos End-to-End (EndToEndWorkflowTests)
- ✅ **Workflow Completo de Usuário**: Registro → Login → Consulta dados
- ✅ **Workflow de Admin**: Criar → Atualizar → Deletar pessoa
- ✅ **Workflow de Segurança**: Validação de permissões por role

## 🔧 Configuração dos Testes

### Banco de Dados de Teste
- **Container**: PostgreSQL 16 Alpine
- **Porta**: 5433 (para não conflitar com instâncias locais)
- **Database**: `peopleconnect_test`
- **Isolamento**: Cada execução usa container limpo

### Usuários de Teste
O seeder cria automaticamente:
- **Admin**: `admin@test.com` / `Admin123!`
- **User**: `user@test.com` / `User123!`
- **Dados**: 10 pessoas de teste com CPFs válidos

### Configurações
- **JWT**: Chave específica para testes
- **Logs**: Nível Warning para reduzir ruído
- **Timeout**: Configurado para aguardar containers

## 🧪 Estrutura de um Teste

```csharp
[Fact]
public async Task CreatePerson_WithValidData_ShouldCreateAndReturnPerson()
{
    // Arrange - Obter token de admin
    var token = await GetAuthTokenAsync();
    SetAuthToken(token);

    var createPersonRequest = new CreatePersonDto
    {
        Name = "Integration Test Person",
        CPF = "12345678909",
        BirthDate = new DateTime(1985, 5, 15),
        Sex = "Female"
    };

    // Act - Executar requisição
    var content = CreateJsonContent(createPersonRequest);
    var response = await Client.PostAsync("/api/v1/persons", content);

    // Assert - Verificar resultado
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    
    var responseContent = await response.Content.ReadAsStringAsync();
    var createdPerson = JsonSerializer.Deserialize<PersonDto>(responseContent, JsonOptions);

    createdPerson.Should().NotBeNull();
    createdPerson!.Name.Should().Be("Integration Test Person");
    createdPerson.CPF.Should().Be("12345678909");
}
```

## 📊 Métricas e Cobertura

### Cobertura de Código
Os testes são configurados para coletar cobertura dos projetos:
- ✅ PeopleConnect.Domain
- ✅ PeopleConnect.Application  
- ✅ PeopleConnect.Infrastructure
- ✅ PeopleConnect.Api

### Relatórios
- **Formato**: Cobertura em formato Cobertura XML
- **Exclusões**: Migrations, arquivos gerados, Program.cs
- **Localização**: `TestResults/coverage.cobertura.xml`

## 🐛 Troubleshooting

### Container não sobe
```bash
# Verificar se Docker está rodando
docker ps

# Limpar containers órfãos
docker container prune

# Verificar logs do container
docker-compose -f Tests/docker-compose.test.yml logs postgres-test
```

### Testes falham por timeout
- Verificar recursos do Docker Desktop
- Aumentar timeout no `IntegrationTestWebAppFactory`
- Verificar se porta 5433 está disponível

### Conflitos de porta
```powershell
# Verificar uso da porta
netstat -ano | findstr :5433

# Matar processo se necessário
taskkill /PID <PID> /F
```

### Problemas de autenticação nos testes
- Verificar se seeder está executando corretamente
- Confirmar credenciais nos testes: `admin@test.com` / `Admin123!`
- Validar configuração JWT no `appsettings.IntegrationTests.json`

## 🔄 CI/CD Integration

Para integração com pipelines de CI/CD:

```yaml
# Exemplo GitHub Actions
- name: Run Integration Tests
  run: |
    cd Backend
    ./run-integration-tests.ps1 -All -WithCoverage
    
- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    file: Backend/TestResults/coverage.cobertura.xml
```

## 📈 Próximos Passos

- [ ] Testes de performance com carga
- [ ] Testes de resiliência (circuit breaker)
- [ ] Testes de segurança (OWASP)
- [ ] Integração com pipeline de CI/CD
- [ ] Testes de migração de banco
- [ ] Testes de backup/restore
