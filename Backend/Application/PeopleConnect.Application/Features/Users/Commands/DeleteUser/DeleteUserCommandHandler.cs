using MediatR;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Domain.Exceptions;

namespace PeopleConnect.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IPersonRepository personRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _personRepository = personRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        
        var userToDelete = await _userRepository.GetByIdWithPersonAsync(request.Id, cancellationToken);
        if (userToDelete == null)
        {
            throw UserException.NotFound(request.Id);
        }

        
        var canDelete = _currentUserService.IsAdmin || _currentUserService.UserId == request.Id;
        if (!canDelete)
        {
            throw AuthorizationException.CannotDeleteOtherUsers(request.Id, _currentUserService.UserId ?? Guid.Empty);
        }

        
        if (userToDelete.IsAdmin() && _currentUserService.UserId == request.Id)
        {
            throw UserException.CannotDeleteSelf();
        }

        
        if (userToDelete.PersonId.HasValue)
        {
            await _personRepository.DeleteAsync(userToDelete.PersonId.Value, cancellationToken);
        }

        
        await _userRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
