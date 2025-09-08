using FluentAssertions;
using FluentValidation.TestHelper;
using PeopleConnect.Application.Features.Persons.Commands.CreatePerson;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Persons.Commands;

public class CreatePersonCommandValidatorTests
{
    private readonly CreatePersonCommandValidator _validator;

    public CreatePersonCommandValidatorTests()
    {
        _validator = new CreatePersonCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
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

        
        var result = _validator.TestValidate(command);

        
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidNome_ShouldHaveError(string? nome)
    {
        
        var command = new CreatePersonCommand(nome!, "11144477735", DateTime.Today.AddYears(-30));

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Nome)
              .WithErrorMessage("Nome é obrigatório");
    }

    [Fact]
    public void Validate_WithLongNome_ShouldHaveError()
    {
        
        var longName = new string('a', 101);
        var command = new CreatePersonCommand(longName, "11144477735", DateTime.Today.AddYears(-30));

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Nome)
              .WithErrorMessage("Nome deve ter no máximo 100 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidCPF_ShouldHaveError(string? cpf)
    {
        
        var command = new CreatePersonCommand("João Silva", cpf!, DateTime.Today.AddYears(-30));

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CPF)
              .WithErrorMessage("CPF é obrigatório");
    }

    [Theory]
    [InlineData("123456789")] 
    [InlineData("123456789012")] 
    public void Validate_WithInvalidCPFLength_ShouldHaveError(string cpf)
    {
        
        var command = new CreatePersonCommand("João Silva", cpf, DateTime.Today.AddYears(-30));

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CPF)
              .WithErrorMessage("CPF deve ter 11 dígitos");
    }

    [Theory]
    [InlineData("1234567890a")]
    [InlineData("123.456.789-01")]
    [InlineData("abc12345678")]
    public void Validate_WithNonNumericCPF_ShouldHaveError(string cpf)
    {
        
        var command = new CreatePersonCommand("João Silva", cpf, DateTime.Today.AddYears(-30));

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CPF)
              .WithErrorMessage("CPF deve conter apenas números");
    }

    [Fact]
    public void Validate_WithFutureBirthDate_ShouldHaveError()
    {
        
        var futureDate = DateTime.Today.AddDays(1);
        var command = new CreatePersonCommand("João Silva", "11144477735", futureDate);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DataNascimento)
              .WithErrorMessage("Data de nascimento não pode ser no futuro");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@invalid.com")]
    [InlineData("user@")]
    [InlineData("user.invalid")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string email)
    {
        
        var command = new CreatePersonCommand("João Silva", "11144477735", DateTime.Today.AddYears(-30), "M", email);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email deve ter um formato válido");
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldNotHaveError()
    {
        
        var command = new CreatePersonCommand("João Silva", "11144477735", DateTime.Today.AddYears(-30), "M", "");

        
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithNullEmail_ShouldNotHaveError()
    {
        
        var command = new CreatePersonCommand("João Silva", "11144477735", DateTime.Today.AddYears(-30), "M", null);

        
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    [InlineData("Masculino")]
    [InlineData("Feminino")]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WithValidSexo_ShouldNotHaveError(string? sexo)
    {
        
        var command = new CreatePersonCommand("João Silva", "11144477735", DateTime.Today.AddYears(-30), sexo);

        
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Sexo);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("Outro")]
    public void Validate_WithInvalidSexo_ShouldHaveError(string sexo)
    {
        
        var command = new CreatePersonCommand("João Silva", "11144477735", DateTime.Today.AddYears(-30), sexo);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Sexo)
              .WithErrorMessage("Sexo deve ser M, F, Masculino ou Feminino");
    }
}
