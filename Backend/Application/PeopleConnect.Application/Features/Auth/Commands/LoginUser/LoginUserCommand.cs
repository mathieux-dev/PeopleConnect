using MediatR;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Auth.Commands.LoginUser;

public record LoginUserCommand(
    string Username,
    string Password
) : IRequest<LoginResponseDto>;
