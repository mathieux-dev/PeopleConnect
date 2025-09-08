using PeopleConnect.Domain.Entities;

namespace PeopleConnect.Application.Dtos;

public record UserResponseDto(
    Guid Id,
    string Username,
    UserRole Role,
    PersonResponseDto? Person,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
