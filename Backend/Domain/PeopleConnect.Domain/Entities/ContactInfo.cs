namespace PeopleConnect.Domain.Entities;

public class ContactInfo
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public Guid PersonId { get; private set; }

    private ContactInfo() { } 

    public ContactInfo(string type, string value, bool isPrimary, Guid personId)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Tipo é obrigatório", nameof(type));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Valor é obrigatório", nameof(value));

        Id = Guid.NewGuid();
        Type = type;
        Value = value;
        IsPrimary = isPrimary;
        PersonId = personId;
    }

    public void Update(string type, string value, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Tipo é obrigatório", nameof(type));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Valor é obrigatório", nameof(value));

        Type = type;
        Value = value;
        IsPrimary = isPrimary;
    }

    public void SetAsPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
