using MyApp.Domain.Models;

namespace MyApp.Application.Interfaces
{
    public interface IPersonRepository
    {
        // CRUD operations for Person entity

        // 'Task' is used for asynchronous programming.

        // Get a person by ID
        Task<Person?> Get(int id);

        // Get all people in the database
        Task<List<Person>> GetAllPeople();

        // Add a new person to the database
        Task Add(Person p);

        // Update an existing person in the database
        Task Update(Person p);

        // Delete a person from the database by ID
        Task Delete(int id);
    }
}