using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleConnect.Application.Dtos;
using PeopleConnect.Application.Features.Persons.Commands.CreatePerson;
using PeopleConnect.Application.Features.Persons.Commands.DeletePerson;
using PeopleConnect.Application.Features.Persons.Commands.UpdatePerson;
using PeopleConnect.Application.Features.Persons.Queries.GetAllPersons;
using PeopleConnect.Application.Features.Persons.Queries.GetPersonById;
using Asp.Versioning;

namespace PeopleConnect.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class PersonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PersonResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PersonResponseDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllPersonsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PersonResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonResponseDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPersonByIdQuery(id));
        
        if (result == null)
            return NotFound("Pessoa não encontrada");

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PersonResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonResponseDto>> Create([FromBody] CreatePersonDto createPersonDto)
    {
        var command = new CreatePersonCommand(
            createPersonDto.Nome,
            createPersonDto.CPF,
            createPersonDto.DataNascimento,
            createPersonDto.Sexo,
            createPersonDto.Email,
            createPersonDto.Naturalidade,
            createPersonDto.Nacionalidade,
            createPersonDto.Telefone,
            createPersonDto.Celular
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PersonResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonResponseDto>> Update(Guid id, [FromBody] UpdatePersonDto updatePersonDto)
    {
        var command = new UpdatePersonCommand(
            id,
            updatePersonDto.Nome,
            updatePersonDto.DataNascimento,
            updatePersonDto.Sexo,
            updatePersonDto.Email,
            updatePersonDto.Naturalidade,
            updatePersonDto.Nacionalidade
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePersonCommand(id));
        return NoContent();
    }
}
