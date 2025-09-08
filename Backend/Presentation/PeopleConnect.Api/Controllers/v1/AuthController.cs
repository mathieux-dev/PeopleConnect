using MediatR;
using Microsoft.AspNetCore.Mvc;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Application.Features.Auth.Commands.LoginUser;
using PeopleConnect.Application.Features.Auth.Commands.RegisterUser;
using Asp.Versioning;

namespace PeopleConnect.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginUserCommand(request.Username, request.Password);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterUserDto request)
    {
        var command = new RegisterUserCommand(request.Username, request.Password, request.Person);
        var result = await _mediator.Send(command);
        
        var loginCommand = new LoginUserCommand(request.Username, request.Password);
        var loginResult = await _mediator.Send(loginCommand);
        
        return CreatedAtAction(nameof(Login), loginResult);
    }
}

public record LoginRequest(string Username, string Password);
