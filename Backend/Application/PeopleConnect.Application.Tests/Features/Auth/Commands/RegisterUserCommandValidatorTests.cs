using FluentAssertions;
using FluentValidation.TestHelper;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Application.Features.Auth.Commands.RegisterUser;
using Xunit;

namespace PeopleConnect.Application.Tests.Features.Auth.Commands;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
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

        
        var result = _validator.TestValidate(command);

        
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidUsername_ShouldHaveError(string? username)
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand(username!, "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username é obrigatório");
    }

    [Theory]
    [InlineData("ab")] 
    [InlineData("a")] 
    public void Validate_WithShortUsername_ShouldHaveError(string username)
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand(username, "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username deve ter pelo menos 3 caracteres");
    }

    [Fact]
    public void Validate_WithLongUsername_ShouldHaveError()
    {
        
        var longUsername = new string('a', 51); 
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand(longUsername, "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username deve ter no máximo 50 caracteres");
    }

    [Theory]
    [InlineData("user@invalid")]
    [InlineData("user with spaces")]
    [InlineData("user#invalid")]
    [InlineData("user$invalid")]
    public void Validate_WithInvalidUsernameFormat_ShouldHaveError(string username)
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand(username, "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username só pode conter letras, números, pontos, hífens e underscores");
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("user.name")]
    [InlineData("user-name")]
    [InlineData("user_name")]
    [InlineData("USER123")]
    public void Validate_WithValidUsernameFormat_ShouldNotHaveError(string username)
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand(username, "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidPassword_ShouldHaveError(string? password)
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("username", password!, personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password é obrigatório");
    }

    [Theory]
    [InlineData("12345")] 
    [InlineData("abc")] 
    public void Validate_WithShortPassword_ShouldHaveError(string password)
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("username", password, personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password deve ter pelo menos 6 caracteres");
    }

    [Fact]
    public void Validate_WithLongPassword_ShouldHaveError()
    {
        
        var longPassword = new string('a', 101); 
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("username", longPassword, personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password deve ter no máximo 100 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidPersonName_ShouldHaveError(string? nome)
    {
        
        var personDto = new CreatePersonDto(nome!, "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Person.Nome)
              .WithErrorMessage("Nome é obrigatório");
    }

    [Fact]
    public void Validate_WithLongPersonName_ShouldHaveError()
    {
        
        var longName = new string('a', 101); 
        var personDto = new CreatePersonDto(longName, "11144477735", DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Person.Nome)
              .WithErrorMessage("Nome deve ter no máximo 100 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidCPF_ShouldHaveError(string? cpf)
    {
        
        var personDto = new CreatePersonDto("João Silva", cpf!, DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Person.CPF)
              .WithErrorMessage("CPF é obrigatório");
    }

    [Theory]
    [InlineData("123456789")] 
    [InlineData("123456789012")] 
    public void Validate_WithInvalidCPFLength_ShouldHaveError(string cpf)
    {
        
        var personDto = new CreatePersonDto("João Silva", cpf, DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Person.CPF)
              .WithErrorMessage("CPF deve ter 11 dígitos");
    }

    [Theory]
    [InlineData("1234567890a")] 
    [InlineData("123.456.789-01")] 
    public void Validate_WithNonNumericCPF_ShouldHaveError(string cpf)
    {
        
        var personDto = new CreatePersonDto("João Silva", cpf, DateTime.Today.AddYears(-30));
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Person.CPF)
              .WithErrorMessage("CPF deve conter apenas números");
    }

    [Fact]
    public void Validate_WithFutureBirthDate_ShouldHaveError()
    {
        
        var futureDate = DateTime.Today.AddDays(1);
        var personDto = new CreatePersonDto("João Silva", "11144477735", futureDate);
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Person.DataNascimento)
              .WithErrorMessage("Data de nascimento não pode ser no futuro");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@invalid.com")]
    [InlineData("user@")]
    [InlineData("user.invalid")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string email)
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30), "M", email);
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Person.Email)
              .WithErrorMessage("Email deve ter um formato válido");
    }

    [Fact]
    public void Validate_WithLongEmail_ShouldHaveError()
    {
        
        var longLocalPart = new string('a', 85); 
        var longEmail = $"{longLocalPart}@email.com"; 
        
        
        var veryLongEmail = new string('a', 95) + "@email.com"; 
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30), "M", veryLongEmail);
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Person.Email)
              .WithErrorMessage("Email deve ter no máximo 100 caracteres");
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldNotHaveError()
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30), "M", "");
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Person.Email);
    }

    [Fact]
    public void Validate_WithNullEmail_ShouldNotHaveError()
    {
        
        var personDto = new CreatePersonDto("João Silva", "11144477735", DateTime.Today.AddYears(-30), "M", null);
        var command = new RegisterUserCommand("username", "password123", personDto);

        
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Person.Email);
    }
}
