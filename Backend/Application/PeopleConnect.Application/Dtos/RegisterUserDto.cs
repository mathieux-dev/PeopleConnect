namespace PeopleConnect.Application.Dtos;

public record RegisterUserDto(
    string Username,
    string Password,
    CreatePersonDto Person
);
