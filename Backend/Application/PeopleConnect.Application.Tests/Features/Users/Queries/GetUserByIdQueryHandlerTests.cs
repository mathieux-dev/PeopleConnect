using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Features.Users.Queries.GetUserById;
using PeopleConnect.Domain.Entities;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Users.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldReturnUserResponseDto()
    {
        
        var userId = Guid.NewGuid();
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be("testuser");
        result.Role.Should().Be(UserRole.User);
        result.Person.Should().BeNull();
        result.CreatedAt.Should().Be(user.CreatedAt);
        result.UpdatedAt.Should().Be(user.UpdatedAt);
    }

    [Fact]
    public async Task Handle_WithExistingUserWithPerson_ShouldReturnUserWithPersonData()
    {
        
        var userId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var person = new Person("João Silva", "11144477735", DateTime.Now.AddYears(-30), "M", "joao@test.com", "São Paulo", "Brasil", creatorUserId);
        user.SetPerson(person);
        
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Person.Should().NotBeNull();
        result.Person!.Nome.Should().Be("João Silva");
        result.Person.CPF.Should().Be("11144477735");
        result.Person.Sexo.Should().Be("M");
        result.Person.Email.Should().Be("joao@test.com");
        result.Person.Naturalidade.Should().Be("São Paulo");
        result.Person.Nacionalidade.Should().Be("Brasil");
    }

    [Fact]
    public async Task Handle_WithUserHavingContacts_ShouldReturnUserWithContactData()
    {
        
        var userId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var person = new Person("João Silva", "11144477735", DateTime.Now.AddYears(-30), "M", "joao@test.com", "São Paulo", "Brasil", creatorUserId);
        
        var contact1 = new ContactInfo("Phone", "(11) 99999-9999", true, person.Id);
        var contact2 = new ContactInfo("Email", "joao.alt@test.com", false, person.Id);
        person.AddContact(contact1);
        person.AddContact(contact2);
        
        user.SetPerson(person);
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Person.Should().NotBeNull();
        result.Person!.Contacts.Should().HaveCount(2);
        
        var contacts = result.Person.Contacts.ToList();
        contacts.Should().Contain(c => c.Type == "Phone" && c.Value == "(11) 99999-9999" && c.IsPrimary);
        contacts.Should().Contain(c => c.Type == "Email" && c.Value == "joao.alt@test.com" && !c.IsPrimary);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnNull()
    {
        
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithAdminUser_ShouldReturnCorrectRole()
    {
        
        var userId = Guid.NewGuid();
        var user = new User("admin", "hashedPassword", UserRole.Admin);
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Role.Should().Be(UserRole.Admin);
        result.Username.Should().Be("admin");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken(true);
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cancellationToken));

        _userRepositoryMock.Verify(x => x.GetByIdWithPersonAsync(userId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllPersonProperties()
    {
        
        var userId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var birthDate = DateTime.Now.AddYears(-25);
        var person = new Person("João Silva", "11144477735", birthDate, "M", "joao@test.com", "São Paulo", "Brasil", creatorUserId);
        user.SetPerson(person);
        
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Person.Should().NotBeNull();
        
        var personResult = result.Person!;
        personResult.Id.Should().Be(person.Id);
        personResult.Nome.Should().Be("João Silva");
        personResult.CPF.Should().Be("11144477735");
        personResult.Sexo.Should().Be("M");
        personResult.Email.Should().Be("joao@test.com");
        personResult.DataNascimento.Should().Be(birthDate);
        personResult.Naturalidade.Should().Be("São Paulo");
        personResult.Nacionalidade.Should().Be("Brasil");
        personResult.CreatedByUserId.Should().Be(creatorUserId);
        personResult.UpdatedByUserId.Should().Be(creatorUserId);
        personResult.CreatedAt.Should().Be(person.CreatedAt);
        personResult.UpdatedAt.Should().Be(person.UpdatedAt);
    }

    [Fact]
    public async Task Handle_WithEmptyContactList_ShouldReturnEmptyContactCollection()
    {
        
        var userId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var person = new Person("João Silva", "11144477735", DateTime.Now.AddYears(-30), "M", "joao@test.com", "São Paulo", "Brasil", creatorUserId);
        user.SetPerson(person);
        
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Person.Should().NotBeNull();
        result.Person!.Contacts.Should().BeEmpty();
    }

    [Theory]
    [InlineData("user123", UserRole.User)]
    [InlineData("admin123", UserRole.Admin)]
    public async Task Handle_WithDifferentUserTypes_ShouldReturnCorrectData(string username, UserRole role)
    {
        
        var userId = Guid.NewGuid();
        var user = new User(username, "hashedPassword", role);
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
        result.Role.Should().Be(role);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOnce()
    {
        
        var userId = Guid.NewGuid();
        var user = new User("testuser", "hashedPassword", UserRole.User);
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock.Setup(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        
        await _handler.Handle(query, CancellationToken.None);

        
        _userRepositoryMock.Verify(x => x.GetByIdWithPersonAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
