namespace PeopleConnect.Application.Dtos;

public record ContactInfoDto(
    Guid Id,
    string Type,
    string Value,
    bool IsPrimary
);
