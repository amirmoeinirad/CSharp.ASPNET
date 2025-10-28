using Microsoft.EntityFrameworkCore;
using MyApp.Application.Interfaces;
using MyApp.Domain.Models;
using MyApp.Infrastructure.Data;

public class PersonRepositoryEF : IPersonRepository
{
    private readonly ApplicationDbContext _db;

    // Constructor to initialize the database context
    // 'db' is provided via dependency injection
    public PersonRepositoryEF(ApplicationDbContext db) => _db = db;


    // Get a person by ID
    // Returns a Task that resolves to a Person or null if not found.
    // FindAsync() returns a ValueTask, so we convert it to Task using AsTask().
    // This allows us to return it directly without 'async' and 'await'.
    public Task<Person?> Get(int id) => _db.People.FindAsync(id).AsTask();


    // Get all people in the database
    // Returns a Task that resolves to a List of Person
    // Since ToListAsync() returns a Task directly, we can return it without 'async' and 'await'.
    // This is more efficient.
    public Task<List<Person>> GetAll() => _db.People.ToListAsync();


    // Add a new person to the database
    public async Task Add(Person p)
    {
        // AddAsync() is asynchronous, so we await it.
        // It adds the entity to the context.
        await _db.AddAsync(p);
        // SaveChangesAsync() is asynchronous, so we await it.
        await _db.SaveChangesAsync(); 
    }


    // Update an existing person in the database
    public async Task Update(Person p)
    {
        // Update() is a synchronous method, so we don't need to await it.
        // It just marks the entity as modified.
        _db.Update(p);
        // SaveChangesAsync() is asynchronous, so we await it.
        await _db.SaveChangesAsync();
    }


    // Delete a person from the database by ID
    public async Task Delete(int id)
    {
        var person = await Get(id);
        if (person is not null)
        {
            // Remove() is a synchronous method, so we don't need to await it.
            // It just marks the entity for deletion.
            _db.Remove(person);
            // SaveChangesAsync() is asynchronous, so we await it.
            await _db.SaveChangesAsync();
        }
    }
}