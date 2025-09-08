namespace PeopleConnect.Application.Dtos;

public record PersonResponseDto(
    Guid Id,
    string Nome,
    string CPF,
    string? Sexo,
    string? Email,
    DateTime DataNascimento,
    string? Naturalidade,
    string? Nacionalidade,
    IReadOnlyCollection<ContactInfoDto> Contacts,
    Guid? CreatedByUserId,
    Guid? UpdatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
