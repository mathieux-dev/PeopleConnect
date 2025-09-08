using MediatR;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Persons.Commands.CreatePerson;

public record CreatePersonCommand(
    string Nome,
    string CPF,
    DateTime DataNascimento,
    string? Sexo = null,
    string? Email = null,
    string? Naturalidade = null,
    string? Nacionalidade = null,
    string? Telefone = null,
    string? Celular = null
) : IRequest<PersonResponseDto>;
