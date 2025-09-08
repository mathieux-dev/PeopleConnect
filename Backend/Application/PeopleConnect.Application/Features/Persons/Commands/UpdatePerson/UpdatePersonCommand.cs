using MediatR;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Persons.Commands.UpdatePerson;

public record UpdatePersonCommand(
    Guid Id,
    string Nome,
    DateTime DataNascimento,
    string? Sexo = null,
    string? Email = null,
    string? Naturalidade = null,
    string? Nacionalidade = null
) : IRequest<PersonResponseDto>;
