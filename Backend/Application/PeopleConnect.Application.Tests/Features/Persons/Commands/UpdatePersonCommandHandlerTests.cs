using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Features.Persons.Commands.UpdatePerson;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Persons.Commands;

public class UpdatePersonCommandHandlerTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdatePersonCommandHandler _handler;
    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();

    public UpdatePersonCommandHandlerTests()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new UpdatePersonCommandHandler(_personRepositoryMock.Object, _currentUserServiceMock.Object);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(_currentUserId);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdatePerson()
    {
        
        var command = new UpdatePersonCommand(
            _personId,
            "João Silva Atualizado",
            DateTime.Today.AddYears(-35),
            "M",
            "joao.novo@email.com",
            "Rio de Janeiro",
            "Brasileira"
        );

        var existingPerson = new Person(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            "M",
            "joao@email.com",
            "São Paulo",
            "Brasileira",
            _currentUserId
        );

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);

        _currentUserServiceMock
            .Setup(x => x.CanEditPerson(command.Id))
            .Returns(true);

        _personRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person person, CancellationToken _) => person);

        
        var result = await _handler.Handle(command, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Id.Should().Be(existingPerson.Id); 
        result.Nome.Should().Be(command.Nome);
        result.DataNascimento.Should().Be(command.DataNascimento);
        result.Sexo.Should().Be(command.Sexo);
        result.Email.Should().Be(command.Email);
        result.Naturalidade.Should().Be(command.Naturalidade);
        result.Nacionalidade.Should().Be(command.Nacionalidade);
        result.UpdatedByUserId.Should().Be(_currentUserId);

        _personRepositoryMock.Verify(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.CanEditPerson(command.Id), Times.Once);
        _personRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPerson_ShouldThrowPersonException()
    {
        
        var command = new UpdatePersonCommand(
            _personId,
            "João Silva",
            DateTime.Today.AddYears(-30)
        );

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        
        var exception = await Assert.ThrowsAsync<PersonException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("PERSON_NOT_FOUND");
        exception.Message.Should().Contain("não foi encontrada");

        _personRepositoryMock.Verify(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.CanEditPerson(It.IsAny<Guid>()), Times.Never);
        _personRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutPermission_ShouldThrowAuthorizationException()
    {
        
        var command = new UpdatePersonCommand(
            _personId,
            "João Silva",
            DateTime.Today.AddYears(-30)
        );

        var existingPerson = new Person(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            "M",
            "joao@email.com",
            "São Paulo",
            "Brasileira",
            Guid.NewGuid() 
        );

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);

        _currentUserServiceMock
            .Setup(x => x.CanEditPerson(command.Id))
            .Returns(false);

        
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("AUTH_CANNOT_EDIT_OTHERS_PERSON");

        _personRepositoryMock.Verify(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.CanEditPerson(command.Id), Times.Once);
        _personRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldUpdatePerson()
    {
        
        var command = new UpdatePersonCommand(
            _personId,
            "Maria Santos",
            DateTime.Today.AddYears(-25)
        );

        var existingPerson = new Person(
            "Maria Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            "F",
            "maria@email.com",
            "São Paulo",
            "Brasileira",
            _currentUserId
        );

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);

        _currentUserServiceMock
            .Setup(x => x.CanEditPerson(command.Id))
            .Returns(true);

        _personRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person person, CancellationToken _) => person);

        
        var result = await _handler.Handle(command, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Nome.Should().Be(command.Nome);
        result.DataNascimento.Should().Be(command.DataNascimento);
        result.Sexo.Should().BeNull(); 
        result.Email.Should().BeNull(); 
        result.Naturalidade.Should().BeNull(); 
        result.Nacionalidade.Should().BeNull(); 
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldPropagateException()
    {
        
        var command = new UpdatePersonCommand(
            _personId,
            "João Silva",
            DateTime.Today.AddYears(-30)
        );

        var existingPerson = new Person(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            createdByUserId: _currentUserId
        );

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerson);

        _currentUserServiceMock
            .Setup(x => x.CanEditPerson(command.Id))
            .Returns(true);

        _personRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationToken()
    {
        
        var command = new UpdatePersonCommand(
            _personId,
            "João Silva",
            DateTime.Today.AddYears(-30)
        );

        var cancellationToken = new CancellationToken(true);

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(command.Id, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationToken));
    }
}
