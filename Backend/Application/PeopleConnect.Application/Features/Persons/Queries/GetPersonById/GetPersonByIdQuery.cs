using MediatR;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Persons.Queries.GetPersonById;

public record GetPersonByIdQuery(Guid Id) : IRequest<PersonResponseDto?>;
