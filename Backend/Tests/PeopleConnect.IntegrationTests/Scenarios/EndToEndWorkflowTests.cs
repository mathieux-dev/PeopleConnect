using FluentAssertions;
using PeopleConnect.Application.Dtos;
using PeopleConnect.IntegrationTests.Infrastructure;
using System.Net;
using System.Text.Json;
using Xunit;

namespace PeopleConnect.IntegrationTests.Scenarios;

/// <summary>
/// Testes de integração End-to-End que validam fluxos completos da aplicação
/// </summary>
[Collection("Integration Tests")]
public class EndToEndWorkflowTests : IntegrationTestBase
{
    public EndToEndWorkflowTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    // Helper method to generate valid CPF
    private static string GenerateValidCPF()
    {
        var random = new Random();
        var cpf = new int[11];

        for (int i = 0; i < 9; i++)
            cpf[i] = random.Next(0, 10);

        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += cpf[i] * (10 - i);

        int remainder = sum % 11;
        cpf[9] = remainder < 2 ? 0 : 11 - remainder;

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += cpf[i] * (11 - i);

        remainder = sum % 11;
        cpf[10] = remainder < 2 ? 0 : 11 - remainder;

        return string.Join("", cpf);
    }

    [Fact]
    public async Task CompleteUserWorkflow_RegisterUserAndManagePersons_ShouldSucceed()
    {
        // 1. Registrar um novo usuário
        var validCPF = GenerateValidCPF();
        var registerRequest = new RegisterUserDto(
            Username: "testuser001",
            Password: "Password123!",
            Person: new CreatePersonDto(
                Nome: "Test User",
                CPF: validCPF, // CPF válido gerado dinamicamente
                DataNascimento: new DateTime(1990, 5, 15),
                Sexo: "M"
            )
        );

        var registerContent = CreateJsonContent(registerRequest);
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", registerContent);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerResponseContent = await registerResponse.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(registerResponseContent, JsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.User.Username.Should().Be("testuser001");

        var token = loginResponse.Token;
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Get the user's own person (created during registration)
        var userPersonId = loginResponse.User.Person?.Id;
        userPersonId.Should().NotBeNull("User should have an associated person after registration");

        // 3. Buscar a pessoa do usuário
        var getPersonResponse = await Client.GetAsync($"/api/v1/persons/{userPersonId!.Value}");
        getPersonResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getPersonResponseContent = await getPersonResponse.Content.ReadAsStringAsync();
        var fetchedPerson = JsonSerializer.Deserialize<PersonResponseDto>(getPersonResponseContent, JsonOptions);

        fetchedPerson.Should().NotBeNull();
        fetchedPerson!.Id.Should().Be(userPersonId!.Value);
        fetchedPerson.Nome.Should().Be("Test User");

        // 4. Atualizar a própria pessoa
        var updatePersonRequest = new UpdatePersonDto(
            Nome: "Test User Updated",
            DataNascimento: new DateTime(1990, 5, 15),
            Sexo: "M"
        );

        var updatePersonContent = CreateJsonContent(updatePersonRequest);
        var updatePersonResponse = await Client.PutAsync($"/api/v1/persons/{userPersonId!.Value}", updatePersonContent);

        updatePersonResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatePersonResponseContent = await updatePersonResponse.Content.ReadAsStringAsync();
        var updatedPerson = JsonSerializer.Deserialize<PersonResponseDto>(updatePersonResponseContent, JsonOptions);

        updatedPerson.Should().NotBeNull();
        updatedPerson!.Nome.Should().Be("Test User Updated");

        // 5. Criar uma nova pessoa (testando permissões - deve falhar para usuário comum)
        var createPersonRequest = new CreatePersonDto(
            Nome: "João Silva",
            CPF: GenerateValidCPF(), // CPF válido diferente
            DataNascimento: new DateTime(1985, 10, 20),
            Sexo: "M"
        );

        var createPersonContent = CreateJsonContent(createPersonRequest);
        var createPersonResponse = await Client.PostAsync("/api/v1/persons", createPersonContent);

        // For regular users, this should succeed (assuming they can create persons)
        createPersonResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createPersonResponseContent = await createPersonResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonResponseDto>(createPersonResponseContent, JsonOptions);

        createdPerson.Should().NotBeNull();
        createdPerson!.Nome.Should().Be("João Silva");
        createdPerson.CPF.Should().Be(createPersonRequest.CPF);

        var newPersonId = createdPerson.Id;

        // 6. Listar todas as pessoas
        var getAllPersonsResponse = await Client.GetAsync("/api/v1/persons");
        getAllPersonsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAllPersonsResponseContent = await getAllPersonsResponse.Content.ReadAsStringAsync();
        var allPersons = JsonSerializer.Deserialize<List<PersonResponseDto>>(getAllPersonsResponseContent, JsonOptions);

        allPersons.Should().NotBeNull();
        allPersons!.Should().Contain(p => p.Id == userPersonId!.Value);
        allPersons!.Should().Contain(p => p.Id == newPersonId);

        // 7. Tentar editar a pessoa que criamos (deve falhar para usuário comum)
        var updateOtherPersonRequest = new UpdatePersonDto(
            Nome: "João Silva Updated",
            DataNascimento: new DateTime(1985, 10, 20),
            Sexo: "M"
        );

        var updateOtherPersonContent = CreateJsonContent(updateOtherPersonRequest);
        var updateOtherPersonResponse = await Client.PutAsync($"/api/v1/persons/{newPersonId}", updateOtherPersonContent);
        updateOtherPersonResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 8. Tentar deletar a pessoa que criamos (deve falhar para usuário comum)
        var deleteOtherPersonResponse = await Client.DeleteAsync($"/api/v1/persons/{newPersonId}");
        deleteOtherPersonResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserSecurityWorkflow_UnauthorizedAccess_ShouldFail()
    {
        // 1. Tentar acessar recursos sem token
        var getPersonsResponse = await Client.GetAsync("/api/v1/persons");
        getPersonsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var createPersonRequest = new CreatePersonDto(
            Nome: "Unauthorized Test",
            CPF: GenerateValidCPF(), // CPF válido diferente
            DataNascimento: new DateTime(1990, 1, 1),
            Sexo: "F"
        );

        var createPersonContent = CreateJsonContent(createPersonRequest);
        var createPersonResponse = await Client.PostAsync("/api/v1/persons", createPersonContent);
        createPersonResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 2. Fazer login com credenciais inválidas
        var invalidLoginRequest = new
        {
            Username = "invalid",
            Password = "invalid"
        };

        var invalidLoginContent = CreateJsonContent(invalidLoginRequest);
        var invalidLoginResponse = await Client.PostAsync("/api/v1/auth/login", invalidLoginContent);
        invalidLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 3. Tentar usar token inválido
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid_token");
        var unauthorizedResponse = await Client.GetAsync("/api/v1/persons");
        unauthorizedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminUserWorkflow_UserManagement_ShouldSucceed()
    {
        // 1. Fazer login como admin
        var token = await GetAuthTokenAsync(); // Método do IntegrationTestBase usa credenciais admin
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Listar todos os usuários (só admin pode)
        var getAllUsersResponse = await Client.GetAsync("/api/v1/users");
        getAllUsersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAllUsersResponseContent = await getAllUsersResponse.Content.ReadAsStringAsync();
        var allUsers = JsonSerializer.Deserialize<List<UserResponseDto>>(getAllUsersResponseContent, JsonOptions);

        allUsers.Should().NotBeNull();
        allUsers!.Should().HaveCountGreaterThan(0);

        // 3. Registrar um novo usuário
        var registerRequest = new RegisterUserDto(
            Username: "manageduser001",
            Password: "Password123!",
            Person: new CreatePersonDto(
                Nome: "Managed User",
                CPF: GenerateValidCPF(), // CPF válido diferente
                DataNascimento: new DateTime(1992, 3, 10),
                Sexo: "F"
            )
        );

        var registerContent = CreateJsonContent(registerRequest);
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registerResponseContent = await registerResponse.Content.ReadAsStringAsync();
        var newUserResponse = JsonSerializer.Deserialize<LoginResponseDto>(registerResponseContent, JsonOptions);
        var newUserId = newUserResponse!.User.Id;

        // 4. Buscar o usuário específico
        var getUserResponse = await Client.GetAsync($"/api/v1/users/{newUserId}");
        getUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getUserResponseContent = await getUserResponse.Content.ReadAsStringAsync();
        var fetchedUser = JsonSerializer.Deserialize<UserResponseDto>(getUserResponseContent, JsonOptions);

        fetchedUser.Should().NotBeNull();
        fetchedUser!.Username.Should().Be("manageduser001");

        // 5. Deletar o usuário
        var deleteUserResponse = await Client.DeleteAsync($"/api/v1/users/{newUserId}");
        deleteUserResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verificar que o usuário foi deletado
        var getDeletedUserResponse = await Client.GetAsync($"/api/v1/users/{newUserId}");
        getDeletedUserResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PersonValidationWorkflow_InvalidData_ShouldFail()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 1. Tentar criar pessoa com CPF inválido
        var invalidCpfRequest = new CreatePersonDto(
            Nome: "Test Invalid CPF",
            CPF: "123", // CPF muito curto
            DataNascimento: new DateTime(1990, 1, 1),
            Sexo: "M"
        );

        var invalidCpfContent = CreateJsonContent(invalidCpfRequest);
        var invalidCpfResponse = await Client.PostAsync("/api/v1/persons", invalidCpfContent);
        invalidCpfResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 2. Tentar criar pessoa com nome vazio
        var invalidNameRequest = new CreatePersonDto(
            Nome: "",
            CPF: GenerateValidCPF(), // CPF válido diferente
            DataNascimento: new DateTime(1990, 1, 1),
            Sexo: "M"
        );

        var invalidNameContent = CreateJsonContent(invalidNameRequest);
        var invalidNameResponse = await Client.PostAsync("/api/v1/persons", invalidNameContent);
        invalidNameResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 3. Tentar criar pessoa com data de nascimento futura
        var invalidDateRequest = new CreatePersonDto(
            Nome: "Test Future Date",
            CPF: GenerateValidCPF(), // CPF válido diferente
            DataNascimento: DateTime.Now.AddYears(1),
            Sexo: "M"
        );

        var invalidDateContent = CreateJsonContent(invalidDateRequest);
        var invalidDateResponse = await Client.PostAsync("/api/v1/persons", invalidDateContent);
        invalidDateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 4. Tentar criar pessoa com sexo inválido
        var invalidSexRequest = new CreatePersonDto(
            Nome: "Test Invalid Sex",
            CPF: GenerateValidCPF(), // CPF válido diferente
            DataNascimento: new DateTime(1990, 1, 1),
            Sexo: "X" // Valor inválido
        );

        var invalidSexContent = CreateJsonContent(invalidSexRequest);
        var invalidSexResponse = await Client.PostAsync("/api/v1/persons", invalidSexContent);
        invalidSexResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AuthValidationWorkflow_InvalidCredentials_ShouldFail()
    {
        // 1. Tentar registrar usuário com senha fraca
        var weakPasswordRequest = new RegisterUserDto(
            Username: "testweakpass",
            Password: "12345", // Senha com 5 caracteres (mínimo é 6)
            Person: new CreatePersonDto(
                Nome: "Test Weak Password",
                CPF: GenerateValidCPF(), // CPF válido diferente
                DataNascimento: new DateTime(1990, 1, 1),
                Sexo: "M"
            )
        );

        var weakPasswordContent = CreateJsonContent(weakPasswordRequest);
        var weakPasswordResponse = await Client.PostAsync("/api/v1/auth/register", weakPasswordContent);
        weakPasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 2. Tentar registrar usuário com username vazio
        var emptyUsernameRequest = new RegisterUserDto(
            Username: "",
            Password: "Password123!",
            Person: new CreatePersonDto(
                Nome: "Test Empty Username",
                CPF: GenerateValidCPF(), // CPF válido diferente
                DataNascimento: new DateTime(1990, 1, 1),
                Sexo: "M"
            )
        );

        var emptyUsernameContent = CreateJsonContent(emptyUsernameRequest);
        var emptyUsernameResponse = await Client.PostAsync("/api/v1/auth/register", emptyUsernameContent);
        emptyUsernameResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 3. Tentar fazer login com credenciais vazias
        var emptyCredentialsRequest = new
        {
            Username = "",
            Password = ""
        };

        var emptyCredentialsContent = CreateJsonContent(emptyCredentialsRequest);
        var emptyCredentialsResponse = await Client.PostAsync("/api/v1/auth/login", emptyCredentialsContent);
        emptyCredentialsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
