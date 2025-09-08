using MediatR;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Persons.Queries.GetAllPersons;

public record GetAllPersonsQuery() : IRequest<IEnumerable<PersonResponseDto>>;
