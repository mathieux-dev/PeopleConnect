namespace PeopleConnect.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public string Code { get; }
    public object? Details { get; }

    protected DomainException(string code, string message, object? details = null) : base(message)
    {
        Code = code;
        Details = details;
    }

    protected DomainException(string code, string message, Exception innerException, object? details = null) 
        : base(message, innerException)
    {
        Code = code;
        Details = details;
    }
}
