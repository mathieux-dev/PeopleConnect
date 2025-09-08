using MediatR;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand(
    string Username,
    string Password,
    CreatePersonDto Person
) : IRequest<UserResponseDto>;
