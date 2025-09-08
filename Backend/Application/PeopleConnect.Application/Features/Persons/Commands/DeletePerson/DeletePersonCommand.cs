using MediatR;

namespace PeopleConnect.Application.Features.Persons.Commands.DeletePerson;

public record DeletePersonCommand(Guid Id) : IRequest;
