using MyApp.Domain.Models;

namespace MyApp.Application.Interfaces
{
    public interface IPersonRepository
    {
        // CRUD operations for Person entity
        // Here, we define the business logic methods for managing Person entities.
        // The actual implementation will be in the Infrastructure layer.

        // 'Task' is used for asynchronous programming.

        // The 'Person' class represents a person entity in the domain model.
        // It is mapped to a database table.


        // (1) Get a person by ID from the database asynchronously.
        Task<Person?> Get(int id);

        // (2) Get all people from the database asynchronously.
        // The method returns a list of Person objects asynchronously.
        Task<List<Person>> GetAllPeople();

        // (3) Add a new person to the database asynchronously.
        Task Add(Person p);

        // (4) Update an existing person in the database asynchronously.
        Task Update(Person p);

        // (5) Delete a person from the database by ID asynchronously.
        Task Delete(int id);
    }
}