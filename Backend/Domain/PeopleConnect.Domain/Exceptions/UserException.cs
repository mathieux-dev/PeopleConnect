namespace PeopleConnect.Domain.Exceptions;




public class UserException : DomainException
{
    public UserException(string code, string message, object? details = null) 
        : base(code, message, details) { }

    public UserException(string code, string message, Exception innerException, object? details = null) 
        : base(code, message, innerException, details) { }

    
    public static UserException NotFound(Guid userId) =>
        new("USER_NOT_FOUND", $"Usuário com ID '{userId}' não foi encontrado", new { UserId = userId });

    public static UserException NotFoundByUsername(string username) =>
        new("USER_NOT_FOUND_BY_USERNAME", $"Usuário com username '{username}' não foi encontrado", new { Username = username });

    public static UserException UsernameAlreadyExists(string username) =>
        new("USER_USERNAME_EXISTS", $"Username '{username}' já está em uso", new { Username = username });

    public static UserException InvalidCredentials() =>
        new("USER_INVALID_CREDENTIALS", "Credenciais inválidas");

    public static UserException WeakPassword(string reason) =>
        new("USER_WEAK_PASSWORD", $"Senha não atende aos critérios de segurança: {reason}", new { Reason = reason });

    public static UserException CannotDeleteSelf() =>
        new("USER_CANNOT_DELETE_SELF", "Administradores não podem deletar sua própria conta");

    public static UserException InvalidUsername(string username, string reason) =>
        new("USER_INVALID_USERNAME", $"Username '{username}' é inválido: {reason}", new { Username = username, Reason = reason });
}
