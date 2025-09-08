using PeopleConnect.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace PeopleConnect.Domain.Tests.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void PersonException_NotFound_ShouldCreateCorrectException()
    {
        
        var personId = Guid.NewGuid();

        
        var exception = PersonException.NotFound(personId);

        
        exception.Code.Should().Be("PERSON_NOT_FOUND");
        exception.Message.Should().Contain(personId.ToString());
        exception.Should().BeAssignableTo<PersonException>();
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void PersonException_InvalidCPF_ShouldCreateCorrectException()
    {
        
        var cpf = "12345678900";

        
        var exception = PersonException.InvalidCPF(cpf);

        
        exception.Code.Should().Be("PERSON_INVALID_CPF");
        exception.Message.Should().Contain("não é válido");
        exception.Message.Should().Contain(cpf);
    }

    [Fact]
    public void PersonException_InvalidBirthDate_ShouldCreateCorrectException()
    {
        
        var birthDate = DateTime.Today.AddDays(1);

        
        var exception = PersonException.InvalidBirthDate(birthDate);

        
        exception.Code.Should().Be("PERSON_INVALID_BIRTH_DATE");
        exception.Message.Should().Contain("não pode ser no futuro");
    }

    [Fact]
    public void UserException_NotFound_ShouldCreateCorrectException()
    {
        
        var userId = Guid.NewGuid();

        
        var exception = UserException.NotFound(userId);

        
        exception.Code.Should().Be("USER_NOT_FOUND");
        exception.Message.Should().Contain(userId.ToString());
        exception.Should().BeAssignableTo<UserException>();
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void UserException_InvalidCredentials_ShouldCreateCorrectException()
    {
        
        var exception = UserException.InvalidCredentials();

        
        exception.Code.Should().Be("USER_INVALID_CREDENTIALS");
        exception.Message.Should().Contain("Credenciais inválidas");
    }

    [Fact]
    public void UserException_AlreadyExists_ShouldCreateCorrectException()
    {
        
        var username = "testuser";

        
        var exception = UserException.UsernameAlreadyExists(username);

        
        exception.Code.Should().Be("USER_USERNAME_EXISTS");
        exception.Message.Should().Contain("já está em uso");
        exception.Message.Should().Contain(username);
    }

    [Fact]
    public void UserException_CannotDeleteSelf_ShouldCreateCorrectException()
    {
        
        var exception = UserException.CannotDeleteSelf();

        
        exception.Code.Should().Be("USER_CANNOT_DELETE_SELF");
        exception.Message.Should().Contain("não podem deletar sua própria conta");
    }

    [Fact]
    public void AuthorizationException_CannotEditOthersPerson_ShouldCreateCorrectException()
    {
        
        var personId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        
        var exception = AuthorizationException.CannotEditOtherUsersPerson(personId, currentUserId);

        
        exception.Code.Should().Be("AUTH_CANNOT_EDIT_OTHERS_PERSON");
        exception.Message.Should().Contain("só pode editar seu próprio perfil");
        exception.Should().BeAssignableTo<AuthorizationException>();
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void AuthorizationException_CannotDeleteOtherUsers_ShouldCreateCorrectException()
    {
        
        var targetUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        
        var exception = AuthorizationException.CannotDeleteOtherUsers(targetUserId, currentUserId);

        
        exception.Code.Should().Be("AUTH_CANNOT_DELETE_OTHER_USERS");
        exception.Message.Should().Contain("só pode deletar sua própria conta");
    }

    [Fact]
    public void AuthorizationException_InsufficientPermissions_ShouldCreateCorrectException()
    {
        
        var operation = "acessar";
        var resource = "painel admin";

        
        var exception = AuthorizationException.InsufficientPermissions(operation, resource);

        
        exception.Code.Should().Be("AUTH_INSUFFICIENT_PERMISSIONS");
        exception.Message.Should().Contain("não tem permissão");
        exception.Message.Should().Contain(operation);
        exception.Message.Should().Contain(resource);
    }

    [Fact]
    public void DomainException_ShouldBeAbstractClass()
    {
        
        typeof(DomainException).IsAbstract.Should().BeTrue();
    }

    [Fact]
    public void DomainException_ShouldInheritFromException()
    {
        
        typeof(DomainException).Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void AllDomainExceptions_ShouldHaveCode()
    {
        
        var personException = PersonException.NotFound(Guid.NewGuid());
        var userException = UserException.InvalidCredentials();
        var authException = AuthorizationException.InsufficientPermissions("test", "resource");

        
        personException.Code.Should().NotBeNullOrEmpty();
        userException.Code.Should().NotBeNullOrEmpty();
        authException.Code.Should().NotBeNullOrEmpty();
    }
}
