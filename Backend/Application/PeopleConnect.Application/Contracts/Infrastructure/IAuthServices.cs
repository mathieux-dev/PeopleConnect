using PeopleConnect.Domain.Entities;

namespace PeopleConnect.Application.Contracts.Infrastructure;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
