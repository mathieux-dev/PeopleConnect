using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Application.Features.Auth.Commands.RegisterUser;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Auth.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _personRepositoryMock = new Mock<IPersonRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _personRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateUserAndPerson()
    {
        
        var personDto = new CreatePersonDto(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            "M",
            "joao@email.com",
            "São Paulo",
            "Brasileira"
        );

        var command = new RegisterUserCommand("joaosilva", "password123", personDto);
        var hashedPassword = "hashed_password_123";
        var expectedUser = new User("joaosilva", hashedPassword, UserRole.User);

        _userRepositoryMock
            .Setup(x => x.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(personDto.CPF, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        _personRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person person, CancellationToken _) => person);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        
        var result = await _handler.Handle(command, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Username.Should().Be(command.Username);
        result.Role.Should().Be(UserRole.User);
        result.Person.Should().NotBeNull();
        result.Person!.Nome.Should().Be(personDto.Nome);
        result.Person.CPF.Should().Be(personDto.CPF);
        result.Person.Email.Should().Be(personDto.Email);

        
        _userRepositoryMock.Verify(x => x.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()), Times.Once);
        _personRepositoryMock.Verify(x => x.CpfExistsAsync(personDto.CPF, null, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(command.Password), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingUsername_ShouldThrowUserException()
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("existinguser", "password123", personDto);

        _userRepositoryMock
            .Setup(x => x.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        
        var exception = await Assert.ThrowsAsync<UserException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("USER_USERNAME_EXISTS");
        exception.Message.Should().Contain("já está em uso");

        _userRepositoryMock.Verify(x => x.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()), Times.Once);
        _personRepositoryMock.Verify(x => x.CpfExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingCPF_ShouldThrowPersonException()
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("newuser", "password123", personDto);

        _userRepositoryMock
            .Setup(x => x.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(personDto.CPF, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        
        var exception = await Assert.ThrowsAsync<PersonException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("PERSON_CPF_EXISTS");
        exception.Message.Should().Contain("já está cadastrado");

        _userRepositoryMock.Verify(x => x.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()), Times.Once);
        _personRepositoryMock.Verify(x => x.CpfExistsAsync(personDto.CPF, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinimalPersonData_ShouldCreateUserSuccessfully()
    {
        
        var personDto = new CreatePersonDto("Maria Santos", "11144477735", DateTime.Today.AddYears(-25));
        var command = new RegisterUserCommand("mariasantos", "password456", personDto);
        var hashedPassword = "hashed_password_456";
        var expectedUser = new User("mariasantos", hashedPassword, UserRole.User);

        _userRepositoryMock
            .Setup(x => x.UsernameExistsAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(personDto.CPF, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        _personRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person person, CancellationToken _) => person);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        
        var result = await _handler.Handle(command, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Person!.Sexo.Should().BeNull();
        result.Person.Email.Should().BeNull();
        result.Person.Naturalidade.Should().BeNull();
        result.Person.Nacionalidade.Should().BeNull();
        result.Person.Contacts.Should().BeEmpty();
    }
}
