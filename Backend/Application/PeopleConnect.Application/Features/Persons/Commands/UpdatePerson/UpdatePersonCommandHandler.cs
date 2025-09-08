using MediatR;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;

namespace PeopleConnect.Application.Features.Persons.Commands.UpdatePerson;

public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, PersonResponseDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePersonCommandHandler(IPersonRepository personRepository, ICurrentUserService currentUserService)
    {
        _personRepository = personRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PersonResponseDto> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = await _personRepository.GetByIdAsync(request.Id, cancellationToken);
        if (person == null)
        {
            throw PersonException.NotFound(request.Id);
        }

        
        if (!_currentUserService.CanEditPerson(request.Id))
        {
            throw AuthorizationException.CannotEditOtherUsersPerson(request.Id, _currentUserService.UserId ?? Guid.Empty);
        }

        person.UpdateInfo(
            request.Nome,
            request.DataNascimento,
            request.Sexo,
            request.Email,
            request.Naturalidade,
            request.Nacionalidade,
            _currentUserService.UserId
        );

        var existingContacts = person.Contacts.ToList();
        
        var existingEmail = existingContacts.FirstOrDefault(c => c.Type == "Email");
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (existingEmail == null)
            {
                var emailContact = new ContactInfo("Email", request.Email, true, person.Id);
                person.AddContact(emailContact);
            }
            else if (existingEmail.Value != request.Email)
            {
                person.RemoveContact(existingEmail.Id);
                var emailContact = new ContactInfo("Email", request.Email, existingEmail.IsPrimary, person.Id);
                person.AddContact(emailContact);
            }
        }
        else if (existingEmail != null)
        {
            person.RemoveContact(existingEmail.Id);
        }

        var existingTelefone = existingContacts.FirstOrDefault(c => c.Type == "Telefone");
        if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            if (existingTelefone == null)
            {
                var telefoneContact = new ContactInfo("Telefone", request.Telefone, true, person.Id);
                person.AddContact(telefoneContact);
            }
            else if (existingTelefone.Value != request.Telefone)
            {
                person.RemoveContact(existingTelefone.Id);
                var telefoneContact = new ContactInfo("Telefone", request.Telefone, existingTelefone.IsPrimary, person.Id);
                person.AddContact(telefoneContact);
            }
        }
        else if (existingTelefone != null)
        {
            person.RemoveContact(existingTelefone.Id);
        }

        var existingCelular = existingContacts.FirstOrDefault(c => c.Type == "Celular");
        if (!string.IsNullOrWhiteSpace(request.Celular))
        {
            if (existingCelular == null)
            {
                var hasTelefone = !string.IsNullOrWhiteSpace(request.Telefone) || existingContacts.Any(c => c.Type == "Telefone");
                var celularContact = new ContactInfo("Celular", request.Celular, !hasTelefone, person.Id);
                person.AddContact(celularContact);
            }
            else if (existingCelular.Value != request.Celular)
            {
                person.RemoveContact(existingCelular.Id);
                var celularContact = new ContactInfo("Celular", request.Celular, existingCelular.IsPrimary, person.Id);
                person.AddContact(celularContact);
            }
        }
        else if (existingCelular != null)
        {
            person.RemoveContact(existingCelular.Id);
        }

        var updatedPerson = await _personRepository.UpdateAsync(person, cancellationToken);

        return new PersonResponseDto(
            updatedPerson.Id,
            updatedPerson.Nome,
            updatedPerson.CPF,
            updatedPerson.Sexo,
            updatedPerson.Email,
            updatedPerson.DataNascimento,
            updatedPerson.Naturalidade,
            updatedPerson.Nacionalidade,
            updatedPerson.Contacts.Select(c => new ContactInfoDto(c.Id, c.Type, c.Value, c.IsPrimary)).ToList(),
            updatedPerson.CreatedByUserId,
            updatedPerson.UpdatedByUserId,
            updatedPerson.CreatedAt,
            updatedPerson.UpdatedAt
        );
    }
}
