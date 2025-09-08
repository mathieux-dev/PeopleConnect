using PeopleConnect.Domain.Exceptions;

namespace PeopleConnect.Domain.Entities;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid? PersonId { get; private set; }
    public Person? Person { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private User() { } 

    public User(string username, string passwordHash, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw UserException.InvalidUsername(username, "Username é obrigatório");

        if (username.Length < 3)
            throw UserException.InvalidUsername(username, "Username deve ter pelo menos 3 caracteres");

        if (username.Length > 50)
            throw UserException.InvalidUsername(username, "Username deve ter no máximo 50 caracteres");

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9._-]+$"))
            throw UserException.InvalidUsername(username, "Username só pode conter letras, números, pontos, hífens e underscores");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash é obrigatório", nameof(passwordHash));

        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPerson(Person person)
    {
        if (person == null)
            throw new ArgumentNullException(nameof(person));

        PersonId = person.Id;
        Person = person;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash é obrigatório", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAdmin() => Role == UserRole.Admin;
    
    public bool CanEditPerson(Guid personId) => IsAdmin() || PersonId == personId;
    
    public bool CanDeletePerson(Guid personId) => IsAdmin() || PersonId == personId;

    public void ValidateCanEditPerson(Guid personId)
    {
        if (!CanEditPerson(personId))
            throw AuthorizationException.CannotEditOtherUsersPerson(personId, Id);
    }

    public void ValidateCanDeleteUser(Guid targetUserId)
    {
        if (!IsAdmin() && Id != targetUserId)
            throw AuthorizationException.CannotDeleteOtherUsers(targetUserId, Id);

        if (IsAdmin() && Id == targetUserId)
            throw UserException.CannotDeleteSelf();
    }
}
