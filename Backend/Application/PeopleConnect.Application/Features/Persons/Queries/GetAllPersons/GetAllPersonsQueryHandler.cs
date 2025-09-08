using MediatR;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Persons.Queries.GetAllPersons;

public class GetAllPersonsQueryHandler : IRequestHandler<GetAllPersonsQuery, IEnumerable<PersonResponseDto>>
{
    private readonly IPersonRepository _personRepository;

    public GetAllPersonsQueryHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<IEnumerable<PersonResponseDto>> Handle(GetAllPersonsQuery request, CancellationToken cancellationToken)
    {
        var persons = await _personRepository.GetAllAsync(cancellationToken);

        return persons.Select(person => new PersonResponseDto(
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
        ));
    }
}
