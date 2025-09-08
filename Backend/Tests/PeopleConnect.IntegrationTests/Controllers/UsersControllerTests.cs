using FluentAssertions;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Domain.Entities;
using PeopleConnect.IntegrationTests.Infrastructure;
using System.Net;
using System.Text.Json;
using Xunit;

namespace PeopleConnect.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class UsersControllerTests : IntegrationTestBase
{
    public UsersControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }
    [Fact]
    public async Task GetAll_WithAdminToken_ShouldReturnUsers()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin", "admin123");
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserResponseDto>>(responseContent, JsonOptions);

        users.Should().NotBeNull();
        users!.Should().HaveCountGreaterThan(0);
        users.Should().Contain(u => u.Username == "admin");
    }

    [Fact]
    public async Task GetAll_WithRegularUserToken_ShouldReturnForbidden()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user", "user123");
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin", "admin123");
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First get all users to get a valid ID
        var getAllResponse = await Client.GetAsync("/api/v1/users");
        var getAllContent = await getAllResponse.Content.ReadAsStringAsync();
        var allUsers = JsonSerializer.Deserialize<List<UserResponseDto>>(getAllContent, JsonOptions);
        var firstUser = allUsers!.First();

        // Act
        var response = await Client.GetAsync($"/api/v1/users/{firstUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserResponseDto>(responseContent, JsonOptions);

        user.Should().NotBeNull();
        user!.Id.Should().Be(firstUser.Id);
        user.Username.Should().Be(firstUser.Username);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin", "admin123");
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/users/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin", "admin123");
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create a user via auth register
        var registerRequest = new RegisterUserDto(
            Username: "usertodelete",
            Password: "Password123!",
            Person: new CreatePersonDto(
                Nome: "User To Delete",
                CPF: "52998224725", // Valid CPF
                DataNascimento: new DateTime(1995, 6, 15),
                Sexo: "Masculino",
                Email: "delete@test.com",
                Naturalidade: "São Paulo",
                Nacionalidade: "Brasileira",
                Telefone: "11987654321",
                Celular: "11987654321"
            )
        );

        var registerContent = CreateJsonContent(registerRequest);
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", registerContent);
        var registerResponseContent = await registerResponse.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(registerResponseContent, JsonOptions);
        var createdUser = loginResponse!.User;

        // Act
        var response = await Client.DeleteAsync($"/api/v1/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user was deleted
        var getResponse = await Client.GetAsync($"/api/v1/users/{createdUser.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin", "admin123");
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/users/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithRegularUserToken_ShouldReturnForbidden()
    {
        // Arrange
        var adminToken = await GetAuthTokenAsync("admin", "admin123");
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        // Get all users to find a user ID that exists
        var getAllResponse = await Client.GetAsync("/api/v1/users");
        var getAllContent = await getAllResponse.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserResponseDto>>(getAllContent, JsonOptions);
        
        // Find a user that is not the current user (which will be "user")
        var targetUser = users!.FirstOrDefault(u => u.Username != "user" && u.Username != "admin");
        if (targetUser == null)
        {
            throw new InvalidOperationException("No suitable target user found for test");
        }

        // Now use regular user token to try to delete the target user (not themselves)
        var userToken = await GetAuthTokenAsync("user", "user123");
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await Client.DeleteAsync($"/api/v1/users/{targetUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
