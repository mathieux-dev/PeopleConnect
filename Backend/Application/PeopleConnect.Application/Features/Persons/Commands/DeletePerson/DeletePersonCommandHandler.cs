using MediatR;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Domain.Exceptions;

namespace PeopleConnect.Application.Features.Persons.Commands.DeletePerson;

public class DeletePersonCommandHandler : IRequestHandler<DeletePersonCommand>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeletePersonCommandHandler(IPersonRepository personRepository, ICurrentUserService currentUserService)
    {
        _personRepository = personRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        if (!await _personRepository.ExistsAsync(request.Id, cancellationToken))
        {
            throw PersonException.NotFound(request.Id);
        }

        
        if (!_currentUserService.CanDeletePerson(request.Id))
        {
            throw AuthorizationException.CannotEditOtherUsersPerson(request.Id, _currentUserService.UserId ?? Guid.Empty);
        }

        await _personRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
