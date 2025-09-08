using MediatR;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Persons.Queries.GetPersonById;

public class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, PersonResponseDto?>
{
    private readonly IPersonRepository _personRepository;

    public GetPersonByIdQueryHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<PersonResponseDto?> Handle(GetPersonByIdQuery request, CancellationToken cancellationToken)
    {
        var person = await _personRepository.GetByIdAsync(request.Id, cancellationToken);

        if (person == null)
            return null;

        return new PersonResponseDto(
            person.Id,
            person.Nome,
            person.CPF,
            person.Sexo,
            person.Email,
            person.DataNascimento,
            person.Naturalidade,
            person.Nacionalidade,
            person.Contacts.Select(c => new ContactInfoDto(c.Id, c.Type, c.Value, c.IsPrimary)).ToList(),
            person.CreatedByUserId,
            person.UpdatedByUserId,
            person.CreatedAt,
            person.UpdatedAt
        );
    }
}
