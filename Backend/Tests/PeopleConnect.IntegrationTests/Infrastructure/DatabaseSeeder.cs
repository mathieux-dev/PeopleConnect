using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Infrastructure.Persistence;
using Bogus;
using PeopleConnect.Application.Contracts.Infrastructure;
using PersonEntity = PeopleConnect.Domain.Entities.Person;

namespace PeopleConnect.IntegrationTests.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.EnsureCreatedAsync();

        // Only seed if we don't have any persons yet (different from checking users)
        if (context.Persons.Any())
            return;

        var persons = new List<PersonEntity>();
        for (int i = 0; i < 10; i++)
        {
            var person = new PersonEntity(
                $"Test Person {i + 1}",
                GenerateValidCPF(),
                DateTime.Now.AddYears(-25 - i),
                i % 2 == 0 ? "Masculino" : "Feminino"
            );
            persons.Add(person);
        }

        var users = new List<User>();
        
        // Don't create admin - already created by DataContext seeding
        // Instead, update the existing admin to have a person
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        if (adminUser != null)
        {
            adminUser.SetPerson(persons[0]);
        }

        // Regular user for testing
        var regularUser = new User(
            "user", 
            passwordHasher.HashPassword("user123"),
            UserRole.User
        );
        regularUser.SetPerson(persons[1]);
        users.Add(regularUser);

        // Additional test users
        for (int i = 2; i < Math.Min(persons.Count, 7); i++)
        {
            var user = new User(
                $"user{i}",
                passwordHasher.HashPassword("test123"),
                UserRole.User
            );
            user.SetPerson(persons[i]);
            users.Add(user);
        }

        await context.Persons.AddRangeAsync(persons);
        await context.Users.AddRangeAsync(users);

        await context.SaveChangesAsync();
    }

    private static string GenerateValidCPF()
    {
        var random = new Random();
        var cpf = new int[11];

        for (int i = 0; i < 9; i++)
            cpf[i] = random.Next(0, 10); // Fixed: should be 0-9 inclusive (0 to 10 exclusive)

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
