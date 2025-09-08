using MediatR;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;

namespace PeopleConnect.Application.Features.Persons.Commands.CreatePerson;

public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, PersonResponseDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreatePersonCommandHandler(IPersonRepository personRepository, ICurrentUserService currentUserService)
    {
        _personRepository = personRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PersonResponseDto> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        
        if (await _personRepository.CpfExistsAsync(request.CPF, null, cancellationToken))
        {
            throw PersonException.CPFAlreadyExists(request.CPF);
        }

        var person = new Person(
            request.Nome,
            request.CPF,
            request.DataNascimento,
            request.Sexo,
            request.Email,
            request.Naturalidade,
            request.Nacionalidade,
            _currentUserService.UserId
        );

        
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailContact = new ContactInfo("Email", request.Email, true, person.Id);
            person.AddContact(emailContact);
        }

        if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            var telefoneContact = new ContactInfo("Telefone", request.Telefone, true, person.Id);
            person.AddContact(telefoneContact);
        }

        if (!string.IsNullOrWhiteSpace(request.Celular))
        {
            var celularContact = new ContactInfo("Celular", request.Celular, !string.IsNullOrWhiteSpace(request.Telefone) ? false : true, person.Id);
            person.AddContact(celularContact);
        }

        var createdPerson = await _personRepository.CreateAsync(person, cancellationToken);

        return new PersonResponseDto(
            createdPerson.Id,
            createdPerson.Nome,
            createdPerson.CPF,
            createdPerson.Sexo,
            createdPerson.Email,
            createdPerson.DataNascimento,
            createdPerson.Naturalidade,
            createdPerson.Nacionalidade,
            createdPerson.Contacts.Select(c => new ContactInfoDto(c.Id, c.Type, c.Value, c.IsPrimary)).ToList(),
            createdPerson.CreatedByUserId,
            createdPerson.UpdatedByUserId,
            createdPerson.CreatedAt,
            createdPerson.UpdatedAt
        );
    }
}
