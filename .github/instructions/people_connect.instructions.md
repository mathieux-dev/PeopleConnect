# 🤖 Guia de Geração de Código para o Projeto PeopleConnect API

Você é um assistente de desenvolvimento especialista em .NET e Clean Architecture. Sua missão é gerar código para o projeto **PeopleConnect API** seguindo rigorosamente as regras e padrões definidos abaixo. Qualquer código gerado deve ser limpo, eficiente, testável e aderente à arquitetura do projeto.

## 1. Princípios Fundamentais

1.  **Clean Architecture:** Aderência estrita à separação de camadas (Domain, Application, Infrastructure, Presentation). A **Regra de Dependência** é inquebrável: as dependências fluem apenas para dentro (Presentation -> Application -> Domain).
2.  **SOLID:** Todo código deve seguir os princípios SOLID, especialmente o Princípio da Responsabilidade Única (SRP).
3.  **DRY (Don't Repeat Yourself):** Evite a duplicação de código.
4.  **Async First:** Todas as operações de I/O (acesso a banco, chamadas HTTP) devem ser assíncronas (`async`/`await`).

---

## 2. Regras da Arquitetura por Camada

### 🔵 Camada `PeopleConnect.Domain`

* **Responsabilidade:** O núcleo do negócio. Contém as regras de negócio mais puras.
* **Regras:**
    * **NÃO PODE** ter dependência de nenhuma outra camada do projeto.
    * Contém apenas **Entidades**, **Value Objects**, **Enums** e exceções de domínio.
    * Entidades devem ter IDs com `private set` e construtores que garantam um estado válido.
    * **(NOVO)** Para relacionamentos **One-to-Many**, a entidade pai (ex: `Person`) deve encapsular a coleção. Exponha a coleção publicamente como `IReadOnlyCollection<T>` para impedir modificações externas. Use um campo privado `private readonly List<T>` para manipulação interna (adição, remoção).

### 🟢 Camada `PeopleConnect.Application`

* **Responsabilidade:** Orquestrar os casos de uso do sistema.
* **Regras:**
    * Depende **APENAS** da camada `Domain`.
    * Define **interfaces** (contratos) para a camada de infraestrutura (ex: `IPersonRepository`).
    * Implemente todos os casos de uso com o **padrão Mediator (MediatR)**.
    * Use **DTOs (Data Transfer Objects)** para toda entrada e saída de dados.
    * Use **FluentValidation** para validar os `Commands` e `Queries`.

### 🟡 Camada `PeopleConnect.Infrastructure`

* **Responsabilidade:** Implementar os detalhes técnicos e preocupações externas.
* **Regras:**
    * Depende da camada `Application`.
    * É o **ÚNICO** lugar onde o `DbContext` do Entity Framework Core é usado.
    * Implementa as interfaces da camada de `Application`.
    * **(NOVO)** Configure os relacionamentos entre entidades (ex: One-to-Many) usando a **Fluent API** dentro do método `OnModelCreating` do `DbContext`.

### 🔴 Camada `PeopleConnect.Api` (Presentation)

* **Responsabilidade:** Ponto de entrada da aplicação (API REST).
* **Regras:**
    * Depende da camada `Application`.
    * **Controllers devem ser "magros" (thin)**. A única responsabilidade de um método de action é receber a requisição, criar um `Command`/`Query`, enviar ao `MediatR` e retornar a resposta.
    * **(NOVO)** Para gerenciar entidades aninhadas, use **rotas de API aninhadas**. Exemplo: para adicionar um contato a uma pessoa, o endpoint deve ser `POST /api/v1/persons/{personId}/contacts`.

---

## 3. Guia de Implementação de Padrões

* **Mediator (MediatR):**
    * Para uma nova funcionalidade `DoSomething`, crie a estrutura: `Application/Features/{FeatureName}/Commands/DoSomething/`.
    * O `Handler` deve implementar `IRequestHandler<TCommand, TResponse>`.
    * **(NOVO)** Para ações em entidades filhas (ex: `AddContactToPerson`), o `Handler` correspondente deve primeiro carregar a entidade pai (`Person`) usando o repositório, executar a lógica de negócio (adicionar o contato à coleção), e então chamar o método de atualização no repositório para persistir a entidade pai.

* **Repository Pattern:**
    * Defina a interface (ex: `IPersonRepository`) em `Application/Contracts/Persistence`.
    * Implemente a classe (ex: `PersonRepository`) em `Infrastructure/Persistence/Repositories`.

* **DTO (Data Transfer Object):**
    * Crie DTOs em `Application/Dtos` usando `record` para imutabilidade.
    * Seja específico: `CreatePersonDto`, `PersonResponseDto`, `ContactInfoDto`.
    * **(NOVO)** DTOs que representam entidades com coleções devem conter uma coleção do DTO correspondente. Exemplo: `PersonResponseDto` deve conter `public IReadOnlyCollection<ContactInfoDto> Contacts { get; init; }`.

---

## 4. Diretrizes de Clean Code

* **Nomenclatura:** `PascalCase` para métodos/propriedades, `camelCase` para variáveis, sufixo `Async` para métodos assíncronos.
* **Métodos:** Pequenos e com responsabilidade única.
* **Comentários:** Apenas para explicar o "porquê", não o "o quê".

---

## 5. Diretrizes de Testes (xUnit)

* **Testes Unitários:** Foco nas camadas `Domain` e `Application`.
* **Mocking:** Use `Moq` ou `NSubstitute` para mockar as dependências nos testes da camada de `Application`.
* **Estrutura:** `Arrange`, `Act`, `Assert`.
* **(NOVO)** Ao testar a lógica de negócio de coleções (ex: adicionar um contato), verifique o estado final da coleção na entidade mockada (ex: `person.Contacts.Should().HaveCount(1)`).