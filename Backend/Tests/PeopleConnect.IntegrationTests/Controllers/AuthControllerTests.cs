using FluentAssertions;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Domain.Entities;
using PeopleConnect.IntegrationTests.Infrastructure;
using System.Net;
using System.Text.Json;
using Xunit;

namespace PeopleConnect.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var loginRequest = new
        {
            Username = "admin",
            Password = "admin123"
        };

        var content = CreateJsonContent(loginRequest);
        var response = await Client.PostAsync("/api/v1/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, JsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Username.Should().Be("admin");
        loginResponse.User.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUserAndReturnToken()
    {
        var registerRequest = new RegisterUserDto(
            Username: "newuser",
            Password: "Password123!",
            Person: new CreatePersonDto(
                Nome: "New User",
                CPF: "11144477735", // CPF válido para teste
                DataNascimento: new DateTime(1990, 1, 1),
                Sexo: "M",
                Email: "newuser@test.com",
                Telefone: "11999999999",
                Celular: "11999999999"
            )
        );

        var content = CreateJsonContent(registerRequest);
        var response = await Client.PostAsync("/api/v1/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, JsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Username.Should().Be("newuser");
        loginResponse.User.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public async Task Register_WithTelefoneAndCelular_ShouldCreateUserWithAllContacts()
    {
        var registerRequest = new RegisterUserDto(
            Username: "usercontacts",
            Password: "Password123!",
            Person: new CreatePersonDto(
                Nome: "User With Contacts",
                CPF: "06541880336", // CPF válido usado em outros testes
                DataNascimento: new DateTime(1990, 1, 1),
                Sexo: "M",
                Email: "usercontacts@test.com",
                Telefone: "1133334444",
                Celular: "11987654321"
            )
        );

        var content = CreateJsonContent(registerRequest);
        var response = await Client.PostAsync("/api/v1/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, JsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.User.Should().NotBeNull();
        
        // Verificar se a pessoa foi criada com os contatos
        var person = loginResponse.User.Person;
        person.Should().NotBeNull();
        person!.Contacts.Should().NotBeNull();
        person.Contacts.Should().HaveCount(3); // Email, Telefone, Celular
        
        // Verificar contatos específicos
        var emailContact = person.Contacts.Should().ContainSingle(c => c.Type == "Email").Subject;
        emailContact.Value.Should().Be("usercontacts@test.com");
        emailContact.IsPrimary.Should().BeTrue();
        
        var telefoneContact = person.Contacts.Should().ContainSingle(c => c.Type == "Telefone").Subject;
        telefoneContact.Value.Should().Be("1133334444");
        telefoneContact.IsPrimary.Should().BeFalse();
        
        var celularContact = person.Contacts.Should().ContainSingle(c => c.Type == "Celular").Subject;
        celularContact.Value.Should().Be("11987654321");
        celularContact.IsPrimary.Should().BeFalse();
    }
}
