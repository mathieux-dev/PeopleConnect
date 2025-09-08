namespace PeopleConnect.IntegrationTests.Models;

public class ErrorResponseDto
{
    public int StatusCode { get; set; }
    public ErrorDetailsDto Error { get; set; } = new();
}

public class ErrorDetailsDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}
