namespace PeopleConnect.Application.Dtos;

public record CreateContactInfoDto(
    string Type,
    string Value,
    bool IsPrimary = false
);
