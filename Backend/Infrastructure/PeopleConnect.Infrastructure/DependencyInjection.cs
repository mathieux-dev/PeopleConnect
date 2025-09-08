using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Infrastructure.Auth;
using PeopleConnect.Infrastructure.Persistence;
using PeopleConnect.Infrastructure.Persistence.Repositories;
using PeopleConnect.Infrastructure.Services;

namespace PeopleConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            
            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase("PeopleConnectDB"));
        }
        else
        {
            
            services.AddDbContext<DataContext>(options =>
                options.UseNpgsql(connectionString, postgresOptions =>
                {
                    postgresOptions.MigrationsAssembly("PeopleConnect.Api");
                }));

            
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
