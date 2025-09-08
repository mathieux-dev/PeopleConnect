using FluentValidation;

namespace PeopleConnect.Application.Features.Persons.Commands.CreatePerson;

public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
{
    public CreatePersonCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.CPF)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Length(11).WithMessage("CPF deve ter 11 dígitos")
            .Matches(@"^\d{11}$").WithMessage("CPF deve conter apenas números");

        RuleFor(x => x.DataNascimento)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Data de nascimento não pode ser no futuro");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Sexo)
            .Must(sexo => string.IsNullOrEmpty(sexo) || new[] { "M", "F", "Masculino", "Feminino" }.Contains(sexo))
            .WithMessage("Sexo deve ser M, F, Masculino ou Feminino");
    }
}
