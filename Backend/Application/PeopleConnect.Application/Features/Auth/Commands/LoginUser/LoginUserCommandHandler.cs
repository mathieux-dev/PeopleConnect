using MediatR;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Domain.Exceptions;

namespace PeopleConnect.Application.Features.Auth.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameWithPersonAsync(request.Username, cancellationToken);
        
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw UserException.InvalidCredentials();
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        
        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserResponseDto(
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
            )
        };
    }
}
