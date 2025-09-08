using FluentAssertions;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Domain.Entities;
using PeopleConnect.IntegrationTests.Infrastructure;
using System.Net;
using System.Text.Json;
using Xunit;

namespace PeopleConnect.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class PersonsControllerTests : IntegrationTestBase
{
    public PersonsControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_WithValidToken_ShouldReturnPersons()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("/api/v1/persons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var persons = JsonSerializer.Deserialize<List<PersonResponseDto>>(responseContent, JsonOptions);

        persons.Should().NotBeNull();
        persons!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/persons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldCreatePerson()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPersonDto = new CreatePersonDto(
            Nome: "Test Person Create",
            CPF: GenerateValidCPF(),
            DataNascimento: new DateTime(1985, 5, 15),
            Sexo: "Feminino",
            Email: "test.create@test.com",
            Naturalidade: "São Paulo",
            Nacionalidade: "Brasileira",
            Telefone: "11987654321",
            Celular: "11987654321"
        );

        var content = CreateJsonContent(createPersonDto);

        // Act
        var response = await Client.PostAsync("/api/v1/persons", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonResponseDto>(responseContent, JsonOptions);

        createdPerson.Should().NotBeNull();
        createdPerson!.Nome.Should().Be("Test Person Create");
        createdPerson.Email.Should().Be("test.create@test.com");
    }

    [Fact]
    public async Task Create_WithInvalidCPF_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPersonDto = new CreatePersonDto(
            Nome: "Test Person",
            CPF: "12345678900", // Invalid CPF
            DataNascimento: new DateTime(1985, 5, 15),
            Sexo: "Masculino",
            Email: "test@test.com",
            Naturalidade: "São Paulo",
            Nacionalidade: "Brasileira",
            Telefone: "11987654321",
            Celular: "11987654321"
        );

        var content = CreateJsonContent(createPersonDto);

        // Act
        var response = await Client.PostAsync("/api/v1/persons", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturnPerson()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create a person
        var createPersonDto = new CreatePersonDto(
            Nome: "Test Person GetById",
            CPF: GenerateValidCPF(),
            DataNascimento: new DateTime(1990, 1, 1),
            Sexo: "Masculino",
            Email: "test.getbyid@test.com",
            Naturalidade: "Rio de Janeiro",
            Nacionalidade: "Brasileira",
            Telefone: "11987654321",
            Celular: "11987654321"
        );

        var createContent = CreateJsonContent(createPersonDto);
        var createResponse = await Client.PostAsync("/api/v1/persons", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonResponseDto>(createResponseContent, JsonOptions);

        // Act
        var response = await Client.GetAsync($"/api/v1/persons/{createdPerson!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var person = JsonSerializer.Deserialize<PersonResponseDto>(responseContent, JsonOptions);

        person.Should().NotBeNull();
        person!.Id.Should().Be(createdPerson.Id);
        person.Nome.Should().Be("Test Person GetById");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/persons/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldUpdatePerson()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create a person
        var createPersonDto = new CreatePersonDto(
            Nome: "Test Person Update Original",
            CPF: GenerateValidCPF(),
            DataNascimento: new DateTime(1988, 3, 10),
            Sexo: "Feminino",
            Email: "test.update@test.com",
            Naturalidade: "Belo Horizonte",
            Nacionalidade: "Brasileira",
            Telefone: "11987654321",
            Celular: "11987654321"
        );

        var createContent = CreateJsonContent(createPersonDto);
        var createResponse = await Client.PostAsync("/api/v1/persons", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonResponseDto>(createResponseContent, JsonOptions);

        // Prepare update data
        var updatePersonDto = new UpdatePersonDto(
            Nome: "Test Person Updated",
            DataNascimento: new DateTime(1988, 3, 10),
            Sexo: "Feminino",
            Email: "test.updated@test.com",
            Naturalidade: "Salvador",
            Nacionalidade: "Brasileira",
            Telefone: "11999887766",
            Celular: "11988776655"
        );

        var updateContent = CreateJsonContent(updatePersonDto);

        // Act
        var response = await Client.PutAsync($"/api/v1/persons/{createdPerson!.Id}", updateContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedPerson = JsonSerializer.Deserialize<PersonResponseDto>(responseContent, JsonOptions);

        updatedPerson.Should().NotBeNull();
        updatedPerson!.Nome.Should().Be("Test Person Updated");
        updatedPerson.Email.Should().Be("test.updated@test.com");
        updatedPerson.Naturalidade.Should().Be("Salvador");
    }

    [Fact]
    public async Task Delete_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create a person
        var createPersonDto = new CreatePersonDto(
            Nome: "Test Person Delete",
            CPF: GenerateValidCPF(),
            DataNascimento: new DateTime(1992, 12, 25),
            Sexo: "Masculino",
            Email: "test.delete@test.com",
            Naturalidade: "Recife",
            Nacionalidade: "Brasileira",
            Telefone: "11987654321",
            Celular: "11987654321"
        );

        var createContent = CreateJsonContent(createPersonDto);
        var createResponse = await Client.PostAsync("/api/v1/persons", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdPerson = JsonSerializer.Deserialize<PersonResponseDto>(createResponseContent, JsonOptions);

        // Act
        var response = await Client.DeleteAsync($"/api/v1/persons/{createdPerson!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify person was deleted
        var getResponse = await Client.GetAsync($"/api/v1/persons/{createdPerson.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/persons/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

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
}
