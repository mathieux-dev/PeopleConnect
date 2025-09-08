using FluentValidation;

namespace PeopleConnect.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username é obrigatório")
            .MinimumLength(3).WithMessage("Username deve ter pelo menos 3 caracteres")
            .MaximumLength(50).WithMessage("Username deve ter no máximo 50 caracteres")
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username só pode conter letras, números, pontos, hífens e underscores");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password é obrigatório")
            .MinimumLength(6).WithMessage("Password deve ter pelo menos 6 caracteres")
            .MaximumLength(100).WithMessage("Password deve ter no máximo 100 caracteres");

        RuleFor(x => x.Person.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Person.CPF)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Length(11).WithMessage("CPF deve ter 11 dígitos")
            .Matches("^[0-9]+$").WithMessage("CPF deve conter apenas números");

        RuleFor(x => x.Person.DataNascimento)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Data de nascimento não pode ser no futuro");

        RuleFor(x => x.Person.Sexo)
            .Must(sexo => string.IsNullOrEmpty(sexo) || new[] { "M", "F", "Masculino", "Feminino" }.Contains(sexo))
            .WithMessage("Sexo deve ser M, F, Masculino ou Feminino");

        When(x => !string.IsNullOrWhiteSpace(x.Person.Email), () =>
        {
            RuleFor(x => x.Person.Email)
                .EmailAddress().WithMessage("Email deve ter um formato válido")
                .MaximumLength(100).WithMessage("Email deve ter no máximo 100 caracteres");
        });
    }
}
