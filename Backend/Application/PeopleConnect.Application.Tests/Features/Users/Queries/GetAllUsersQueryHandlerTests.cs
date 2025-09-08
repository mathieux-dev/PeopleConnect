using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Application.Features.Users.Queries.GetAllUsers;
using PeopleConnect.Domain.Entities;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Users.Queries;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingUsers_ShouldReturnUserResponseDtos()
    {
        
        var user1 = new User("user1", "hash1", UserRole.User);
        var user2 = new User("admin1", "hash2", UserRole.Admin);
        var users = new List<User> { user1, user2 };
        
        var query = new GetAllUsersQuery();

        _userRepositoryMock.Setup(x => x.GetAllWithPersonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var resultList = result.ToList();
        resultList[0].Username.Should().Be("user1");
        resultList[0].Role.Should().Be(UserRole.User);
        resultList[0].Person.Should().BeNull();
        
        resultList[1].Username.Should().Be("admin1");
        resultList[1].Role.Should().Be(UserRole.Admin);
        resultList[1].Person.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUsersHavingPersons_ShouldReturnUsersWithPersonData()
    {
        
        var creatorUserId = Guid.NewGuid();
        var user1 = new User("user1", "hash1", UserRole.User);
        var person1 = new Person("João Silva", "11144477735", DateTime.Now.AddYears(-30), "M", "joao@test.com", "São Paulo", "Brasil", creatorUserId);
        user1.SetPerson(person1);

        var user2 = new User("user2", "hash2", UserRole.User);
        var person2 = new Person("Maria Santos", "98765432100", DateTime.Now.AddYears(-25), "F", "maria@test.com", "Rio de Janeiro", "Brasil", creatorUserId);
        user2.SetPerson(person2);

        var users = new List<User> { user1, user2 };
        var query = new GetAllUsersQuery();

        _userRepositoryMock.Setup(x => x.GetAllWithPersonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var resultList = result.ToList();
        
        resultList[0].Person.Should().NotBeNull();
        resultList[0].Person!.Nome.Should().Be("João Silva");
        resultList[0].Person!.CPF.Should().Be("11144477735");
        resultList[0].Person!.Email.Should().Be("joao@test.com");
        
        resultList[1].Person.Should().NotBeNull();
        resultList[1].Person!.Nome.Should().Be("Maria Santos");
        resultList[1].Person!.CPF.Should().Be("98765432100");
        resultList[1].Person!.Email.Should().Be("maria@test.com");
    }

    [Fact]
    public async Task Handle_WithUsersHavingContacts_ShouldReturnUsersWithContactData()
    {
        
        var creatorUserId = Guid.NewGuid();
        var user = new User("user1", "hash1", UserRole.User);
        var person = new Person("João Silva", "11144477735", DateTime.Now.AddYears(-30), "M", "joao@test.com", "São Paulo", "Brasil", creatorUserId);
        
        var contact1 = new ContactInfo("Phone", "(11) 99999-9999", true, person.Id);
        var contact2 = new ContactInfo("Email", "joao.alt@test.com", false, person.Id);
        person.AddContact(contact1);
        person.AddContact(contact2);
        
        user.SetPerson(person);
        var users = new List<User> { user };
        var query = new GetAllUsersQuery();

        _userRepositoryMock.Setup(x => x.GetAllWithPersonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var userResult = result.First();
        userResult.Person.Should().NotBeNull();
        userResult.Person!.Contacts.Should().HaveCount(2);
        
        var contacts = userResult.Person.Contacts.ToList();
        contacts.Should().Contain(c => c.Type == "Phone" && c.Value == "(11) 99999-9999" && c.IsPrimary);
        contacts.Should().Contain(c => c.Type == "Email" && c.Value == "joao.alt@test.com" && !c.IsPrimary);
    }

    [Fact]
    public async Task Handle_WithEmptyRepository_ShouldReturnEmptyCollection()
    {
        
        var users = new List<User>();
        var query = new GetAllUsersQuery();

        _userRepositoryMock.Setup(x => x.GetAllWithPersonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMixedUsersAndPersons_ShouldReturnCorrectMixture()
    {
        
        var creatorUserId = Guid.NewGuid();
        
        var userWithPerson = new User("user1", "hash1", UserRole.User);
        var person = new Person("João Silva", "11144477735", DateTime.Now.AddYears(-30), "M", "joao@test.com", "São Paulo", "Brasil", creatorUserId);
        userWithPerson.SetPerson(person);

        var userWithoutPerson = new User("user2", "hash2", UserRole.Admin);

        var users = new List<User> { userWithPerson, userWithoutPerson };
        var query = new GetAllUsersQuery();

        _userRepositoryMock.Setup(x => x.GetAllWithPersonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var resultList = result.ToList();
        resultList[0].Person.Should().NotBeNull();
        resultList[1].Person.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        
        var cancellationToken = new CancellationToken(true);
        var query = new GetAllUsersQuery();

        _userRepositoryMock.Setup(x => x.GetAllWithPersonsAsync(cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cancellationToken));

        _userRepositoryMock.Verify(x => x.GetAllWithPersonsAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllUserProperties()
    {
        
        var user = new User("testuser", "hashedPassword", UserRole.Admin);
        var users = new List<User> { user };
        var query = new GetAllUsersQuery();

        _userRepositoryMock.Setup(x => x.GetAllWithPersonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        var userResult = result.First();
        userResult.Id.Should().Be(user.Id);
        userResult.Username.Should().Be(user.Username);
        userResult.Role.Should().Be(user.Role);
        userResult.CreatedAt.Should().Be(user.CreatedAt);
        userResult.UpdatedAt.Should().Be(user.UpdatedAt);
    }

    [Fact]
    public async Task Handle_WithDifferentUserRoles_ShouldReturnCorrectRoles()
    {
        
        var regularUser = new User("regular", "hash1", UserRole.User);
        var adminUser = new User("admin", "hash2", UserRole.Admin);
        var users = new List<User> { regularUser, adminUser };
        var query = new GetAllUsersQuery();

        _userRepositoryMock.Setup(x => x.GetAllWithPersonsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        var resultList = result.ToList();
        resultList.Should().Contain(u => u.Role == UserRole.User);
        resultList.Should().Contain(u => u.Role == UserRole.Admin);
    }
}
