using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Features.Persons.Commands.DeletePerson;
using PeopleConnect.Domain.Exceptions;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Persons.Commands;

public class DeletePersonCommandHandlerTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeletePersonCommandHandler _handler;
    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();

    public DeletePersonCommandHandlerTests()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new DeletePersonCommandHandler(_personRepositoryMock.Object, _currentUserServiceMock.Object);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(_currentUserId);
    }

    [Fact]
    public async Task Handle_WithValidIdAndPermission_ShouldDeletePerson()
    {
        
        var command = new DeletePersonCommand(_personId);

        _personRepositoryMock
            .Setup(x => x.ExistsAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _currentUserServiceMock
            .Setup(x => x.CanDeletePerson(command.Id))
            .Returns(true);

        _personRepositoryMock
            .Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        
        await _handler.Handle(command, CancellationToken.None);

        
        _personRepositoryMock.Verify(x => x.ExistsAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.CanDeletePerson(command.Id), Times.Once);
        _personRepositoryMock.Verify(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPerson_ShouldThrowPersonException()
    {
        
        var command = new DeletePersonCommand(_personId);

        _personRepositoryMock
            .Setup(x => x.ExistsAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        
        var exception = await Assert.ThrowsAsync<PersonException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("PERSON_NOT_FOUND");
        exception.Message.Should().Contain("não foi encontrada");

        _personRepositoryMock.Verify(x => x.ExistsAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.CanDeletePerson(It.IsAny<Guid>()), Times.Never);
        _personRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutPermission_ShouldThrowAuthorizationException()
    {
        
        var command = new DeletePersonCommand(_personId);

        _personRepositoryMock
            .Setup(x => x.ExistsAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _currentUserServiceMock
            .Setup(x => x.CanDeletePerson(command.Id))
            .Returns(false);

        
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Code.Should().Be("AUTH_CANNOT_EDIT_OTHERS_PERSON");

        _personRepositoryMock.Verify(x => x.ExistsAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.CanDeletePerson(command.Id), Times.Once);
        _personRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryDeleteFails_ShouldPropagateException()
    {
        
        var command = new DeletePersonCommand(_personId);

        _personRepositoryMock
            .Setup(x => x.ExistsAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _currentUserServiceMock
            .Setup(x => x.CanDeletePerson(command.Id))
            .Returns(true);

        _personRepositoryMock
            .Setup(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _personRepositoryMock.Verify(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationToken()
    {
        
        var command = new DeletePersonCommand(_personId);
        var cancellationToken = new CancellationToken(true);

        _personRepositoryMock
            .Setup(x => x.ExistsAsync(command.Id, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldCheckExistenceBeforePermission()
    {
        
        var command = new DeletePersonCommand(_personId);

        _personRepositoryMock
            .Setup(x => x.ExistsAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        
        await Assert.ThrowsAsync<PersonException>(
            () => _handler.Handle(command, CancellationToken.None));

        
        _currentUserServiceMock.Verify(x => x.CanDeletePerson(It.IsAny<Guid>()), Times.Never);
    }
}
