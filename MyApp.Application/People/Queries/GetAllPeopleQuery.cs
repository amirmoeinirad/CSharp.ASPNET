using MediatR;

namespace MyApp.Application.People.Queries
{
    // IRequest is part of the MediatR library.
    // It represents a request that can be handled by a handler.
    // A request can be either a command (which changes state) or a query (which retrieves data).
    // The philosophy of separating commands and queries is known as CQRS (Command Query Responsibility Segregation).
    // CQRS is a design pattern that separates read and write operations for a data store.
    public record GetAllPeopleQuery() : IRequest<List<PersonDto>>;
}