using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Features.Persons.Queries.GetPersonById;
using PeopleConnect.Domain.Entities;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Persons.Queries;

public class GetPersonByIdQueryHandlerTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly GetPersonByIdQueryHandler _handler;
    private readonly Guid _personId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public GetPersonByIdQueryHandlerTests()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _handler = new GetPersonByIdQueryHandler(_personRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingPerson_ShouldReturnPersonDto()
    {
        
        var query = new GetPersonByIdQuery(_personId);
        var person = new Person(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            "M",
            "joao@email.com",
            "São Paulo",
            "Brasileira",
            _userId
        );

        
        var contact = new ContactInfo("telefone", "11999999999", true, person.Id);
        person.AddContact(contact);

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Id.Should().Be(person.Id);
        result.Nome.Should().Be(person.Nome);
        result.CPF.Should().Be(person.CPF);
        result.DataNascimento.Should().Be(person.DataNascimento);
        result.Sexo.Should().Be(person.Sexo);
        result.Email.Should().Be(person.Email);
        result.Naturalidade.Should().Be(person.Naturalidade);
        result.Nacionalidade.Should().Be(person.Nacionalidade);
        result.CreatedByUserId.Should().Be(person.CreatedByUserId);
        result.UpdatedByUserId.Should().Be(person.UpdatedByUserId);
        result.CreatedAt.Should().Be(person.CreatedAt);
        result.UpdatedAt.Should().Be(person.UpdatedAt);

        
        result.Contacts.Should().HaveCount(1);
        result.Contacts.First().Type.Should().Be("telefone");
        result.Contacts.First().Value.Should().Be("11999999999");
        result.Contacts.First().IsPrimary.Should().BeTrue();

        _personRepositoryMock.Verify(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPerson_ShouldReturnNull()
    {
        
        var query = new GetPersonByIdQuery(_personId);

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().BeNull();
        _personRepositoryMock.Verify(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPersonWithoutOptionalFields_ShouldReturnDtoWithNulls()
    {
        
        var query = new GetPersonByIdQuery(_personId);
        var person = new Person(
            "Maria Santos",
            "11144477735",
            DateTime.Today.AddYears(-25),
            createdByUserId: _userId
        );

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Maria Santos");
        result.CPF.Should().Be("11144477735");
        result.Sexo.Should().BeNull();
        result.Email.Should().BeNull();
        result.Naturalidade.Should().BeNull();
        result.Nacionalidade.Should().BeNull();
        result.Contacts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPersonWithMultipleContacts_ShouldReturnAllContacts()
    {
        
        var query = new GetPersonByIdQuery(_personId);
        var person = new Person(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            createdByUserId: _userId
        );

        var contact1 = new ContactInfo("telefone", "11999999999", true, person.Id);
        var contact2 = new ContactInfo("email", "joao@email.com", false, person.Id);
        var contact3 = new ContactInfo("telefone", "11888888888", false, person.Id);

        person.AddContact(contact1);
        person.AddContact(contact2);
        person.AddContact(contact3);

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result!.Contacts.Should().HaveCount(3);
        
        var phoneContacts = result.Contacts.Where(c => c.Type == "telefone").ToList();
        phoneContacts.Should().HaveCount(2);
        phoneContacts.Should().Contain(c => c.Value == "11999999999" && c.IsPrimary);
        phoneContacts.Should().Contain(c => c.Value == "11888888888" && !c.IsPrimary);

        var emailContacts = result.Contacts.Where(c => c.Type == "email").ToList();
        emailContacts.Should().HaveCount(1);
        emailContacts.First().Value.Should().Be("joao@email.com");
        emailContacts.First().IsPrimary.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        
        var query = new GetPersonByIdQuery(_personId);

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationToken()
    {
        
        var query = new GetPersonByIdQuery(_personId);
        var cancellationToken = new CancellationToken(true);

        _personRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cancellationToken));
    }
}
