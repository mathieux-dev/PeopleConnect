namespace PeopleConnect.Application.Dtos;

public record UpdatePersonDto(
    string Nome,
    DateTime DataNascimento,
    string? Sexo = null,
    string? Email = null,
    string? Naturalidade = null,
    string? Nacionalidade = null,
    string? Telefone = null,
    string? Celular = null
);
