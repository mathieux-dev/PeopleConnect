using MediatR;
using PeopleConnect.Application.Dtos;

namespace PeopleConnect.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<IEnumerable<UserResponseDto>>;
