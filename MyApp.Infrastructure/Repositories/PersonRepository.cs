using Microsoft.EntityFrameworkCore;
using MyApp.Application.Interfaces;
using MyApp.Domain.Models;
using MyApp.Infrastructure.Data;

public class PersonRepository : IPersonRepository
{
    private readonly ApplicationDbContext _db;

    // Constructor to initialize the database context
    // 'db' is provided via dependency injection
    public PersonRepository(ApplicationDbContext db) => _db = db;

    // Get a person by ID
    // Returns a Task that resolves to a Person or null if not found
    public Task<Person?> Get(int id) => _db.People.FindAsync(id).AsTask();

    // Get all people in the database
    // Returns a Task that resolves to a List of Person
    // Since ToListAsync() returns a Task directly, we can return it without 'async' and 'await'
    public Task<List<Person>> GetAll() => _db.People.ToListAsync();

    // Add a new person to the database
    public async Task Add(Person p)
    { 
        await _db.AddAsync(p); 
        await _db.SaveChangesAsync(); 
    }
}