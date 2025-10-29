using Dapper;
using MyApp.Domain.Models;
using System.Data;

public class PersonRepositoryDapper
{
    // Database connection field
    // 'IDbConnection' is an interface for database connections in ADO.NET.
    // Dapper works with ADO.NET IDbConnection to execute SQL queries.
    private readonly IDbConnection _db;


    // Constructor to initialize the database connection.    
    // 'db' is provided via dependency injection.
    public PersonRepositoryDapper(IDbConnection db) => _db = db;


    // Here, we use Dapper for faster data access instead of Entity Framework.
    // GetAll() in PersonRepositoryEF.cs uses EF Core to get all Person records.
    // So, GetAll() and GetPeople() have the same purpose but different implementations.
    public Task<IEnumerable<Person>> GetAllPeople()
    {
        // QueryAsync is an asynchronous method from Dapper to execute a SQL query.
        // It is added to the IDbConnection interface via Dapper's extension methods.
        return _db.QueryAsync<Person>("SELECT Id, FirstName, LastName, CreatedAt, UpdatedAt FROM People");
    }
}