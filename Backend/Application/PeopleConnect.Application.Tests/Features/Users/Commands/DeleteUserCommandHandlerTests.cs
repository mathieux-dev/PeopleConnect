using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Features.Users.Commands.DeleteUser;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Users.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _personRepositoryMock = new Mock<IPersonRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new DeleteUserCommandHandler(
            _userRepositoryMock.Object,
            _personRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserAndAdmin_ShouldDeleteUserAndPerson()
    {
        
        var userId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var person = new Person("Test User", "11144477735", DateTime.Now.AddYears(-25), "M", "test@test.com", "São Paulo", "Brasil", currentUserId);
        user.SetPerson(person);
        
        var command = new DeleteUserCommand(userId);

        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        await _handler.Handle(command, CancellationToken.None);

        
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _personRepositoryMock.Verify(x => x.DeleteAsync(person.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidUserAndOwner_ShouldDeleteUserAndPerson()
    {
        
        var userId = Guid.NewGuid();
        var currentUserId = userId; 
        
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var person = new Person("Test User", "11144477735", DateTime.Now.AddYears(-25), "M", "test@test.com", "São Paulo", "Brasil", currentUserId);
        user.SetPerson(person);
        
        var command = new DeleteUserCommand(userId);

        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        await _handler.Handle(command, CancellationToken.None);

        
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _personRepositoryMock.Verify(x => x.DeleteAsync(person.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserWithoutPerson_ShouldDeleteOnlyUser()
    {
        
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var command = new DeleteUserCommand(userId);

        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        await _handler.Handle(command, CancellationToken.None);

        
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _personRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUserException()
    {
        
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        
        var exception = await Assert.ThrowsAsync<UserException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("USER_NOT_FOUND");
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldThrowAuthorizationException()
    {
        
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid(); 
        
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var command = new DeleteUserCommand(userId);

        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("AUTH_CANNOT_DELETE_OTHER_USERS");
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAdminDeletingSelf_ShouldThrowUserException()
    {
        
        var userId = Guid.NewGuid();
        var currentUserId = userId; 
        
        var user = new User("admin", "hashedPassword", UserRole.Admin);
        var command = new DeleteUserCommand(userId);

        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var exception = await Assert.ThrowsAsync<UserException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("USER_CANNOT_DELETE_SELF");
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullCurrentUserId_ShouldThrowAuthorizationException()
    {
        
        var userId = Guid.NewGuid();
        
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var command = new DeleteUserCommand(userId);

        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("AUTH_CANNOT_DELETE_OTHER_USERS");
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(true, false)] 
    [InlineData(false, true)] 
    public async Task Handle_WithValidPermissions_ShouldSucceed(bool isAdmin, bool isSameUser)
    {
        
        var userId = Guid.NewGuid();
        var currentUserId = isSameUser ? userId : Guid.NewGuid();
        
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var command = new DeleteUserCommand(userId);

        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(isAdmin);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        await _handler.Handle(command, CancellationToken.None);

        
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositories()
    {
        
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var cancellationToken = new CancellationToken(true);
        
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var person = new Person("Test User", "11144477735", DateTime.Now.AddYears(-25), "M", "test@test.com", "São Paulo", "Brasil", currentUserId);
        user.SetPerson(person);
        
        var command = new DeleteUserCommand(userId);

        _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationToken));

        _userRepositoryMock.Verify(x => x.GetByIdWithPersonAsync(userId, cancellationToken), Times.Once);
    }
}
