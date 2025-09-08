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

        // Atualizar contatos específicos - remover apenas os tipos que estão sendo atualizados
        var contactTypesToUpdate = new[] { "Email", "Telefone", "Celular" };
        var contactsToRemove = person.Contacts.Where(c => contactTypesToUpdate.Contains(c.Type)).ToList();
        
        foreach (var contact in contactsToRemove)
        {
            person.RemoveContact(contact.Id);
        }

        // Adicionar novos contatos
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
