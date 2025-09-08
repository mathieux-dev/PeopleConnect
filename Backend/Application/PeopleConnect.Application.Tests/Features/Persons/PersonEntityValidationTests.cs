using FluentAssertions;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Persons;

public class PersonEntityValidationTests
{
    [Fact]
    public void Person_WithValidData_ShouldCreateSuccessfully()
    {
        
        var nome = "João Silva";
        var cpf = "11144477735";
        var dataNascimento = DateTime.Today.AddYears(-30);
        var sexo = "M";
        var email = "joao@email.com";

        
        var person = new Person(nome, cpf, dataNascimento, sexo, email);

        
        person.Nome.Should().Be(nome);
        person.CPF.Should().Be(cpf);
        person.DataNascimento.Should().Be(dataNascimento);
        person.Sexo.Should().Be(sexo);
        person.Email.Should().Be(email);
        person.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Person_WithInvalidCPF_ShouldThrowException()
    {
        
        var nome = "João Silva";
        var invalidCpf = "12345678900"; 
        var dataNascimento = DateTime.Today.AddYears(-30);

        
        var exception = Assert.Throws<PersonException>(() => new Person(nome, invalidCpf, dataNascimento));
        exception.Code.Should().Be("PERSON_INVALID_CPF");
    }

    [Fact]
    public void Person_WithFutureBirthDate_ShouldThrowException()
    {
        
        var nome = "João Silva";
        var cpf = "11144477735";
        var futureDate = DateTime.Today.AddDays(1);

        
        var exception = Assert.Throws<PersonException>(() => new Person(nome, cpf, futureDate));
        exception.Code.Should().Be("PERSON_INVALID_BIRTH_DATE");
    }

    [Fact]
    public void Person_UpdateInfo_ShouldUpdateFieldsCorrectly()
    {
        
        var person = new Person("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var newNome = "João Santos";
        var newDataNascimento = DateTime.Today.AddYears(-25);
        var newSexo = "M";
        var newEmail = "joao.santos@email.com";
        var originalUpdatedAt = person.UpdatedAt;

        
        Thread.Sleep(10);

        
        person.UpdateInfo(newNome, newDataNascimento, newSexo, newEmail);

        
        person.Nome.Should().Be(newNome);
        person.DataNascimento.Should().Be(newDataNascimento);
        person.Sexo.Should().Be(newSexo);
        person.Email.Should().Be(newEmail);
        person.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Person_AddContact_ShouldAddContactSuccessfully()
    {
        
        var person = new Person("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var contact = new ContactInfo("Email", "joao@email.com", false, person.Id);

        
        person.AddContact(contact);

        
        person.Contacts.Should().HaveCount(1);
        person.Contacts.Should().Contain(contact);
    }

    [Fact]
    public void Person_RemoveContact_ShouldRemoveContactSuccessfully()
    {
        
        var person = new Person("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var contact = new ContactInfo("Email", "joao@email.com", false, person.Id);
        person.AddContact(contact);

        
        person.RemoveContact(contact.Id);

        
        person.Contacts.Should().BeEmpty();
    }
}
