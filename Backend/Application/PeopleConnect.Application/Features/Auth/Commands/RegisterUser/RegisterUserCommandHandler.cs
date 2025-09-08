using MediatR;
using PeopleConnect.Application.Contracts.Infrastructure;
using PeopleConnect.Application.Contracts.Persistence;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Domain.Entities;
using PeopleConnect.Domain.Exceptions;

namespace PeopleConnect.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPersonRepository personRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _personRepository = personRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        
        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw UserException.UsernameAlreadyExists(request.Username);
        }

        
        if (await _personRepository.CpfExistsAsync(request.Person.CPF, null, cancellationToken))
        {
            throw PersonException.CPFAlreadyExists(request.Person.CPF);
        }

        
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        
        var user = new User(request.Username, passwordHash, UserRole.User);
        await _userRepository.CreateAsync(user, cancellationToken);

        
        var person = new Person(
            request.Person.Nome,
            request.Person.CPF,
            request.Person.DataNascimento,
            request.Person.Sexo,
            request.Person.Email,
            request.Person.Naturalidade,
            request.Person.Nacionalidade,
            user.Id
        );

        await _personRepository.CreateAsync(person, cancellationToken);

        
        user.SetPerson(person);
        await _userRepository.UpdateAsync(user, cancellationToken);

        
        return new UserResponseDto(
            user.Id,
            user.Username,
            user.Role,
            new PersonResponseDto(
                person.Id,
                person.Nome,
                person.CPF,
                person.Sexo,
                person.Email,
                person.DataNascimento,
                person.Naturalidade,
                person.Nacionalidade,
                new List<ContactInfoDto>(),
                person.CreatedByUserId,
                person.UpdatedByUserId,
                person.CreatedAt,
                person.UpdatedAt
            ),
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}
