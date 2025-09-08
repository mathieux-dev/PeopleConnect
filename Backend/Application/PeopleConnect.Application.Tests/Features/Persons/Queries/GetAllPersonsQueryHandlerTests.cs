using FluentAssertions;
using Moq;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Features.Persons.Queries.GetAllPersons;
using PeopleConnect.Domain.Entities;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Persons.Queries;

public class GetAllPersonsQueryHandlerTests
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly GetAllPersonsQueryHandler _handler;
    private readonly Guid _userId1 = Guid.NewGuid();
    private readonly Guid _userId2 = Guid.NewGuid();

    public GetAllPersonsQueryHandlerTests()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _handler = new GetAllPersonsQueryHandler(_personRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithMultiplePersons_ShouldReturnAllPersonDtos()
    {
        
        var query = new GetAllPersonsQuery();

        var person1 = new Person(
            "João Silva",
            "11144477735",
            DateTime.Today.AddYears(-30),
            "M",
            "joao@email.com",
            "São Paulo",
            "Brasileira",
            _userId1
        );

        var person2 = new Person(
            "Maria Santos",
            "98765432100", 
            DateTime.Today.AddYears(-25),
            "F",
            "maria@email.com",
            "Rio de Janeiro",
            "Brasileira",
            _userId2
        );

        var person3 = new Person(
            "Pedro Costa",
            "12345678909", 
            DateTime.Today.AddYears(-35),
            createdByUserId: _userId1
        );

        
        var contact1 = new ContactInfo("telefone", "11999999999", true, person1.Id);
        var contact2 = new ContactInfo("email", "joao.trabalho@empresa.com", false, person1.Id);
        person1.AddContact(contact1);
        person1.AddContact(contact2);

        var contact3 = new ContactInfo("telefone", "21888888888", true, person2.Id);
        person2.AddContact(contact3);

        var persons = new List<Person> { person1, person2, person3 };

        _personRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        var resultList = result.ToList();
        resultList.Should().HaveCount(3);

        
        var dto1 = resultList.First(p => p.Nome == "João Silva");
        dto1.CPF.Should().Be("11144477735");
        dto1.Sexo.Should().Be("M");
        dto1.Email.Should().Be("joao@email.com");
        dto1.Naturalidade.Should().Be("São Paulo");
        dto1.Nacionalidade.Should().Be("Brasileira");
        dto1.CreatedByUserId.Should().Be(_userId1);
        dto1.Contacts.Should().HaveCount(2);

        
        var dto2 = resultList.First(p => p.Nome == "Maria Santos");
        dto2.CPF.Should().Be("98765432100");
        dto2.Sexo.Should().Be("F");
        dto2.Email.Should().Be("maria@email.com");
        dto2.CreatedByUserId.Should().Be(_userId2);
        dto2.Contacts.Should().HaveCount(1);

        
        var dto3 = resultList.First(p => p.Nome == "Pedro Costa");
        dto3.CPF.Should().Be("12345678909");
        dto3.Sexo.Should().BeNull();
        dto3.Email.Should().BeNull();
        dto3.Naturalidade.Should().BeNull();
        dto3.Nacionalidade.Should().BeNull();
        dto3.CreatedByUserId.Should().Be(_userId1);
        dto3.Contacts.Should().BeEmpty();

        _personRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyDatabase_ShouldReturnEmptyCollection()
    {
        
        var query = new GetAllPersonsQuery();
        var emptyPersons = new List<Person>();

        _personRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPersons);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _personRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSinglePerson_ShouldReturnSingleDto()
    {
        
        var query = new GetAllPersonsQuery();
        var person = new Person(
            "Ana Oliveira",
            "11144477735", 
            DateTime.Today.AddYears(-28),
            "F",
            "ana@email.com",
            createdByUserId: _userId1
        );

        var persons = new List<Person> { person };

        _personRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        
        var dto = resultList.First();
        dto.Nome.Should().Be("Ana Oliveira");
        dto.CPF.Should().Be("11144477735");
        dto.Sexo.Should().Be("F");
        dto.Email.Should().Be("ana@email.com");
        dto.CreatedByUserId.Should().Be(_userId1);
    }

    [Fact]
    public async Task Handle_WithPersonsWithDifferentContactCounts_ShouldMapContactsCorrectly()
    {
        
        var query = new GetAllPersonsQuery();

        var personWithNoContacts = new Person(
            "Sem Contatos",
            "11144477735", 
            DateTime.Today.AddYears(-30),
            createdByUserId: _userId1
        );

        var personWithOneContact = new Person(
            "Um Contato",
            "98765432100", 
            DateTime.Today.AddYears(-30),
            createdByUserId: _userId1
        );
        personWithOneContact.AddContact(new ContactInfo("email", "um@email.com", true, personWithOneContact.Id));

        var personWithMultipleContacts = new Person(
            "Múltiplos Contatos",
            "12345678909", 
            DateTime.Today.AddYears(-30),
            createdByUserId: _userId1
        );
        personWithMultipleContacts.AddContact(new ContactInfo("telefone", "11999999999", true, personWithMultipleContacts.Id));
        personWithMultipleContacts.AddContact(new ContactInfo("email", "multiplos@email.com", false, personWithMultipleContacts.Id));
        personWithMultipleContacts.AddContact(new ContactInfo("telefone", "11888888888", false, personWithMultipleContacts.Id));

        var persons = new List<Person> { personWithNoContacts, personWithOneContact, personWithMultipleContacts };

        _personRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        var resultList = result.ToList();
        
        var noContactsDto = resultList.First(p => p.Nome == "Sem Contatos");
        noContactsDto.Contacts.Should().BeEmpty();

        var oneContactDto = resultList.First(p => p.Nome == "Um Contato");
        oneContactDto.Contacts.Should().HaveCount(1);
        oneContactDto.Contacts.First().Type.Should().Be("email");

        var multipleContactsDto = resultList.First(p => p.Nome == "Múltiplos Contatos");
        multipleContactsDto.Contacts.Should().HaveCount(3);
        multipleContactsDto.Contacts.Should().Contain(c => c.Type == "telefone" && c.IsPrimary);
        multipleContactsDto.Contacts.Should().Contain(c => c.Type == "email" && !c.IsPrimary);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        
        var query = new GetAllPersonsQuery();

        _personRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationToken()
    {
        
        var query = new GetAllPersonsQuery();
        var cancellationToken = new CancellationToken(true);

        _personRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cancellationToken));
    }

    [Fact]
    public async Task Handle_ShouldMapAllPersonPropertiesCorrectly()
    {
        
        var query = new GetAllPersonsQuery();
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var updatedAt = DateTime.UtcNow.AddDays(-5);
        
        var person = new Person(
            "Teste Completo",
            "11144477735", 
            DateTime.Today.AddYears(-40),
            "M",
            "teste@email.com",
            "Brasília",
            "Brasileira",
            _userId1
        );

        var persons = new List<Person> { person };

        _personRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons);

        
        var result = await _handler.Handle(query, CancellationToken.None);

        
        var dto = result.First();
        dto.Id.Should().Be(person.Id);
        dto.Nome.Should().Be(person.Nome);
        dto.CPF.Should().Be(person.CPF);
        dto.Sexo.Should().Be(person.Sexo);
        dto.Email.Should().Be(person.Email);
        dto.DataNascimento.Should().Be(person.DataNascimento);
        dto.Naturalidade.Should().Be(person.Naturalidade);
        dto.Nacionalidade.Should().Be(person.Nacionalidade);
        dto.CreatedByUserId.Should().Be(person.CreatedByUserId);
        dto.UpdatedByUserId.Should().Be(person.UpdatedByUserId);
        dto.CreatedAt.Should().Be(person.CreatedAt);
        dto.UpdatedAt.Should().Be(person.UpdatedAt);
    }
}
