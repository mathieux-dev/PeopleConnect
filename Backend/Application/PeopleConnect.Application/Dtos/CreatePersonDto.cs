namespace PeopleConnect.Application.Dtos;

public record CreatePersonDto(
    string Nome,
    string CPF,
    DateTime DataNascimento,
    string? Sexo = null,
    string? Email = null,
    string? Naturalidade = null,
    string? Nacionalidade = null,
    string? Telefone = null,
    string? Celular = null
);
