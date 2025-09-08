using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Features.Auth.Commands.LoginUser;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Auth.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _handler = new LoginUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnToken()
    {
        
        var command = new LoginUserCommand("testuser", "password123");
        var user = new User("testuser", "hashedpassword", UserRole.User);
        var expectedToken = "jwt-token-123";

        _userRepositoryMock
            .Setup(x => x.GetByUsernameWithPersonAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(user))
            .Returns(expectedToken);

        
        var result = await _handler.Handle(command, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedToken);
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be(user.Username);
        result.User.Role.Should().Be(user.Role);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        
        _userRepositoryMock.Verify(x => x.GetByUsernameWithPersonAsync(command.Username, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(command.Password, user.PasswordHash), Times.Once);
        _jwtTokenGeneratorMock.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUserException()
    {
        
        var command = new LoginUserCommand("nonexistent", "password123");

        _userRepositoryMock
            .Setup(x => x.GetByUsernameWithPersonAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        
        var exception = await Assert.ThrowsAsync<UserException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("USER_INVALID_CREDENTIALS");
        exception.Message.Should().Contain("Credenciais inválidas");

        _userRepositoryMock.Verify(x => x.GetByUsernameWithPersonAsync(command.Username, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowUserException()
    {
        
        var command = new LoginUserCommand("testuser", "wrongpassword");
        var user = new User("testuser", "hashedpassword", UserRole.User);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameWithPersonAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        
        var exception = await Assert.ThrowsAsync<UserException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("USER_INVALID_CREDENTIALS");
        exception.Message.Should().Contain("Credenciais inválidas");

        _userRepositoryMock.Verify(x => x.GetByUsernameWithPersonAsync(command.Username, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(command.Password, user.PasswordHash), Times.Once);
        _jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData("", "password123")]
    [InlineData("testuser", "")]
    [InlineData(null, "password123")]
    [InlineData("testuser", null)]
    public async Task Handle_WithEmptyCredentials_ShouldStillCallRepository(string? username, string? password)
    {
        
        var command = new LoginUserCommand(username!, password!);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameWithPersonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        
        var exception = await Assert.ThrowsAsync<UserException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("USER_INVALID_CREDENTIALS");
        _userRepositoryMock.Verify(x => x.GetByUsernameWithPersonAsync(username!, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationToken()
    {
        
        var command = new LoginUserCommand("testuser", "password123");
        var cancellationToken = new CancellationToken(true);

        _userRepositoryMock
            .Setup(x => x.GetByUsernameWithPersonAsync(command.Username, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationToken));
    }
}
