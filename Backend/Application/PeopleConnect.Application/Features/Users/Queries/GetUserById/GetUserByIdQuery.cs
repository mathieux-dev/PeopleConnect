using MediatR;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<UserResponseDto?>;
