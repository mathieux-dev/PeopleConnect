using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PeopleConnect.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using DotNet.Testcontainers.Builders;
using Xunit;

namespace PeopleConnect.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("peopleconnect_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(0, 5432)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DataContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<DataContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });

        builder.UseEnvironment("IntegrationTests");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Create and seed the database
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestWebAppFactory>>();

        try
        {
            await context.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred creating/seeding the test database");
            throw;
        }
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }
}
