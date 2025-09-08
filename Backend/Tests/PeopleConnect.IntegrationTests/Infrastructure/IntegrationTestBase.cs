using Microsoft.Extensions.DependencyInjection;
using PeopleConnect.Infrastructure.Persistence;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace PeopleConnect.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task<string> GetAuthTokenAsync(string email = "admin", string password = "admin123")
    {
        var loginRequest = new
        {
            Username = email,
            Password = password
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await Client.PostAsync("/api/v1/auth/login", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed: {response.StatusCode} - {errorContent}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseContent);
        var root = document.RootElement;
        
        if (root.TryGetProperty("token", out var tokenElement))
        {
            return tokenElement.GetString() ?? throw new InvalidOperationException("Token is null");
        }
        
        throw new InvalidOperationException("Token not found in response");
    }

    protected async Task<string> GetUserAuthTokenAsync()
    {
        return await GetAuthTokenAsync("user@test.com", "User123!");
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task ClearDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        
        context.Users.RemoveRange(context.Users);
        context.Persons.RemoveRange(context.Persons);
        await context.SaveChangesAsync();
    }

    protected StringContent CreateJsonContent(object obj)
    {
        return new StringContent(
            JsonSerializer.Serialize(obj, JsonOptions),
            Encoding.UTF8,
            "application/json");
    }

    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }
}
