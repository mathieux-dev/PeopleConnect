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
}
