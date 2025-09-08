using System.Security.Claims;

namespace PeopleConnect.Application.Contracts.Infrastructure;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool CanEditPerson(Guid personId);
    bool CanDeletePerson(Guid personId);
}
