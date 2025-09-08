using MediatR;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponseDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithPersonAsync(request.Id, cancellationToken);

        if (user == null)
            return null;

        return new UserResponseDto(
            user.Id,
            user.Username,
            user.Role,
            user.Person != null ? new PersonResponseDto(
                user.Person.Id,
                user.Person.Nome,
                user.Person.CPF,
                user.Person.Sexo,
                user.Person.Email,
                user.Person.DataNascimento,
                user.Person.Naturalidade,
                user.Person.Nacionalidade,
                user.Person.Contacts.Select(c => new ContactInfoDto(c.Id, c.Type, c.Value, c.IsPrimary)).ToList(),
                user.Person.CreatedByUserId,
                user.Person.UpdatedByUserId,
                user.Person.CreatedAt,
                user.Person.UpdatedAt
            ) : null,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}
