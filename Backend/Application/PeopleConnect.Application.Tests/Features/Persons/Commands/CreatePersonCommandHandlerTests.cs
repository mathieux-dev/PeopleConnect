using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Features.Persons.Commands.CreatePerson;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Persons.Commands;

public class CreatePersonCommandHandlerTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreatePersonCommandHandler _handler;
    private readonly Guid _currentUserId = Guid.NewGuid();

    public CreatePersonCommandHandlerTests()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new CreatePersonCommandHandler(_personRepositoryMock.Object, _currentUserServiceMock.Object);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(_currentUserId);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreatePerson()
    {
        
        var command = new CreatePersonCommand(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            "M",
            "joao@email.com",
            "São Paulo",
            "Brasileira"
        );

        var createdPerson = new Person(
            command.Nome,
            command.CPF,
            command.DataNascimento,
            command.Sexo,
            command.Email,
            command.Naturalidade,
            command.Nacionalidade,
            _currentUserId
        );

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(command.CPF, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _personRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPerson);

        
        var result = await _handler.Handle(command, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Nome.Should().Be(command.Nome);
        result.CPF.Should().Be(command.CPF);
        result.DataNascimento.Should().Be(command.DataNascimento);
        result.Sexo.Should().Be(command.Sexo);
        result.Email.Should().Be(command.Email);
        result.Naturalidade.Should().Be(command.Naturalidade);
        result.Nacionalidade.Should().Be(command.Nacionalidade);
        result.CreatedByUserId.Should().Be(_currentUserId);
        result.Contacts.Should().BeEmpty();

        _personRepositoryMock.Verify(x => x.CpfExistsAsync(command.CPF, null, It.IsAny<CancellationToken>()), Times.Once);
        _personRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldCreatePerson()
    {
        
        var command = new CreatePersonCommand(
            "Maria Santos",
            "11144477735",
            DateTime.Today.AddYears(-25)
        );

        var createdPerson = new Person(
            command.Nome,
            command.CPF,
            command.DataNascimento,
            command.Sexo,
            command.Email,
            command.Naturalidade,
            command.Nacionalidade,
            _currentUserId
        );

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(command.CPF, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _personRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPerson);

        
        var result = await _handler.Handle(command, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Nome.Should().Be(command.Nome);
        result.CPF.Should().Be(command.CPF);
        result.DataNascimento.Should().Be(command.DataNascimento);
        result.Sexo.Should().BeNull();
        result.Email.Should().BeNull();
        result.Naturalidade.Should().BeNull();
        result.Nacionalidade.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithExistingCPF_ShouldThrowPersonException()
    {
        
        var command = new CreatePersonCommand(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30)
        );

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(command.CPF, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        
        var exception = await Assert.ThrowsAsync<PersonException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("PERSON_CPF_EXISTS");
        exception.Message.Should().Contain("já está cadastrado");

        _personRepositoryMock.Verify(x => x.CpfExistsAsync(command.CPF, null, It.IsAny<CancellationToken>()), Times.Once);
        _personRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldPropagateException()
    {
        
        var command = new CreatePersonCommand(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30)
        );

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(command.CPF, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _personRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationToken()
    {
        
        var command = new CreatePersonCommand(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30)
        );

        var cancellationToken = new CancellationToken(true);

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(command.CPF, null, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentUserIdAsCreatedBy()
    {
        
        var command = new CreatePersonCommand(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30)
        );

        var createdPerson = new Person(
            command.Nome,
            command.CPF,
            command.DataNascimento,
            command.Sexo,
            command.Email,
            command.Naturalidade,
            command.Nacionalidade,
            _currentUserId
        );

        _personRepositoryMock
            .Setup(x => x.CpfExistsAsync(command.CPF, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _personRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPerson);

        
        var result = await _handler.Handle(command, CancellationToken.None);

        
        result.CreatedByUserId.Should().Be(_currentUserId);
        _currentUserServiceMock.Verify(x => x.UserId, Times.Once);
    }
}
