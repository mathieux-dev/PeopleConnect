using PeopleConnect.Domain.Exceptions;

namespace PeopleConnect.Domain.Entities;

public class Person
{
    private readonly List<ContactInfo> _contacts = new();

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string CPF { get; private set; } = string.Empty;
    public string? Sexo { get; private set; }
    public string? Email { get; private set; }
    public DateTime DataNascimento { get; private set; }
    public string? Naturalidade { get; private set; }
    public string? Nacionalidade { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<ContactInfo> Contacts => _contacts.AsReadOnly();

    private Person() { } 

    public Person(
        string nome,
        string cpf,
        DateTime dataNascimento,
        string? sexo = null,
        string? email = null,
        string? naturalidade = null,
        string? nacionalidade = null,
        Guid? createdByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório", nameof(nome));

        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF é obrigatório", nameof(cpf));

        if (!IsValidCPF(cpf))
            throw PersonException.InvalidCPF(cpf);

        if (dataNascimento > DateTime.Today)
            throw PersonException.InvalidBirthDate(dataNascimento);

        Id = Guid.NewGuid();
        Nome = nome;
        CPF = cpf;
        DataNascimento = dataNascimento;
        Sexo = sexo;
        Email = email;
        Naturalidade = naturalidade;
        Nacionalidade = nacionalidade;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(
        string nome,
        DateTime dataNascimento,
        string? sexo = null,
        string? email = null,
        string? naturalidade = null,
        string? nacionalidade = null,
        Guid? updatedByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório", nameof(nome));

        if (dataNascimento > DateTime.Today)
            throw PersonException.InvalidBirthDate(dataNascimento);

        Nome = nome;
        DataNascimento = dataNascimento;
        Sexo = sexo;
        Email = email;
        Naturalidade = naturalidade;
        Nacionalidade = nacionalidade;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddContact(ContactInfo contact)
    {
        if (contact == null)
            throw new ArgumentNullException(nameof(contact));

        
        if (contact.IsPrimary)
        {
            foreach (var existingContact in _contacts.Where(c => c.Type == contact.Type))
            {
                existingContact.SetAsPrimary(false);
            }
        }

        _contacts.Add(contact);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveContact(Guid contactId)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId);
        if (contact != null)
        {
            _contacts.Remove(contact);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateContact(Guid contactId, string type, string value, bool isPrimary)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId);
        if (contact == null)
            throw PersonException.ContactNotFound(contactId);

        
        if (isPrimary)
        {
            foreach (var existingContact in _contacts.Where(c => c.Type == type && c.Id != contactId))
            {
                existingContact.SetAsPrimary(false);
            }
        }

        contact.Update(type, value, isPrimary);
        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidCPF(string cpf)
    {
        
        cpf = System.Text.RegularExpressions.Regex.Replace(cpf, @"[^\d]", "");

        
        if (cpf.Length != 11)
            return false;

        
        if (cpf.All(c => c == cpf[0]))
            return false;

        
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += int.Parse(cpf[i].ToString()) * (10 - i);

        int remainder = sum % 11;
        int digit1 = remainder < 2 ? 0 : 11 - remainder;

        if (int.Parse(cpf[9].ToString()) != digit1)
            return false;

        
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += int.Parse(cpf[i].ToString()) * (11 - i);

        remainder = sum % 11;
        int digit2 = remainder < 2 ? 0 : 11 - remainder;

        return int.Parse(cpf[10].ToString()) == digit2;
    }
}
