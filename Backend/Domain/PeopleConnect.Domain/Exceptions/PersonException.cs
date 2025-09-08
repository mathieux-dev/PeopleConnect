namespace PeopleConnect.Domain.Exceptions;

public class PersonException : DomainException
{
    public PersonException(string code, string message, object? details = null) 
        : base(code, message, details) { }

    public PersonException(string code, string message, Exception innerException, object? details = null) 
        : base(code, message, innerException, details) { }

    
    public static PersonException NotFound(Guid personId) =>
        new("PERSON_NOT_FOUND", $"Pessoa com ID '{personId}' não foi encontrada", new { PersonId = personId });

    public static PersonException CPFAlreadyExists(string cpf) =>
        new("PERSON_CPF_EXISTS", $"CPF '{cpf}' já está cadastrado no sistema", new { CPF = cpf });

    public static PersonException InvalidCPF(string cpf) =>
        new("PERSON_INVALID_CPF", $"CPF '{cpf}' não é válido", new { CPF = cpf });

    public static PersonException InvalidBirthDate(DateTime birthDate) =>
        new("PERSON_INVALID_BIRTH_DATE", $"Data de nascimento '{birthDate:dd/MM/yyyy}' não pode ser no futuro", new { BirthDate = birthDate });

    public static PersonException ContactNotFound(Guid contactId) =>
        new("PERSON_CONTACT_NOT_FOUND", $"Contato com ID '{contactId}' não foi encontrado", new { ContactId = contactId });

    public static PersonException InvalidContactType(string type) =>
        new("PERSON_INVALID_CONTACT_TYPE", $"Tipo de contato '{type}' não é válido", new { Type = type });
}
