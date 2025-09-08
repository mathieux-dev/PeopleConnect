using PeopleConnect.Domain.Exceptions;
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace PeopleConnect.Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Error = new ErrorDetails
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Dados de entrada inválidos",
                    Details = validationEx.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                };
                break;

            case DomainException domainEx:
                response.StatusCode = GetStatusCodeForDomainException(domainEx);
                response.Error = new ErrorDetails
                {
                    Code = domainEx.Code,
                    Message = domainEx.Message,
                    Details = domainEx.Details
                };
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Error = new ErrorDetails
                {
                    Code = "UNAUTHORIZED",
                    Message = "Acesso não autorizado",
                    Details = null
                };
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Error = new ErrorDetails
                {
                    Code = "INVALID_ARGUMENT",
                    Message = argEx.Message,
                    Details = new { Parameter = argEx.ParamName }
                };
                break;

            case InvalidOperationException invOpEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Error = new ErrorDetails
                {
                    Code = "INVALID_OPERATION",
                    Message = invOpEx.Message,
                    Details = null
                };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Error = new ErrorDetails
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Message = "Ocorreu um erro interno no servidor",
                    Details = null
                };
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static int GetStatusCodeForDomainException(DomainException domainException)
    {
        return domainException switch
        {
            PersonException when domainException.Code.Contains("NOT_FOUND") => (int)HttpStatusCode.NotFound,
            UserException when domainException.Code.Contains("NOT_FOUND") => (int)HttpStatusCode.NotFound,
            UserException when domainException.Code == "USER_INVALID_CREDENTIALS" => (int)HttpStatusCode.Unauthorized,
            AuthorizationException => (int)HttpStatusCode.Forbidden,
            _ when domainException.Code.Contains("EXISTS") => (int)HttpStatusCode.Conflict,
            _ when domainException.Code.Contains("INVALID") => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.BadRequest
        };
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public ErrorDetails Error { get; set; } = null!;
}

public class ErrorDetails
{
    public string Code { get; set; } = null!;
    public string Message { get; set; } = null!;
    public object? Details { get; set; }
}
