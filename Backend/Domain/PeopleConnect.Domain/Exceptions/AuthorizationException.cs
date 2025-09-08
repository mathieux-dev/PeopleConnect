namespace PeopleConnect.Domain.Exceptions;




public class AuthorizationException : DomainException
{
    public AuthorizationException(string code, string message, object? details = null) 
        : base(code, message, details) { }

    public AuthorizationException(string code, string message, Exception innerException, object? details = null) 
        : base(code, message, innerException, details) { }

    
    public static AuthorizationException InsufficientPermissions(string operation, string resource) =>
        new("AUTH_INSUFFICIENT_PERMISSIONS", 
            $"Você não tem permissão para executar a operação '{operation}' no recurso '{resource}'", 
            new { Operation = operation, Resource = resource });

    public static AuthorizationException NotAuthenticated() =>
        new("AUTH_NOT_AUTHENTICATED", "Você precisa estar autenticado para acessar este recurso");

    public static AuthorizationException TokenExpired() =>
        new("AUTH_TOKEN_EXPIRED", "Seu token de acesso expirou, faça login novamente");

    public static AuthorizationException InvalidToken() =>
        new("AUTH_INVALID_TOKEN", "Token de acesso inválido");

    public static AuthorizationException CannotEditOtherUsersPerson(Guid personId, Guid currentUserId) =>
        new("AUTH_CANNOT_EDIT_OTHERS_PERSON", 
            "Você só pode editar seu próprio perfil ou ser um administrador", 
            new { PersonId = personId, CurrentUserId = currentUserId });

    public static AuthorizationException CannotDeleteOtherUsers(Guid targetUserId, Guid currentUserId) =>
        new("AUTH_CANNOT_DELETE_OTHER_USERS", 
            "Você só pode deletar sua própria conta ou ser um administrador", 
            new { TargetUserId = targetUserId, CurrentUserId = currentUserId });
}
