using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Models;

using NodaTime;

namespace MyApp.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        // FIELDS ////////////////////////////////////
        // 'NodaTime.IClock' is injected to get the current time
        // It returns 'NodaTime.Instant' for UTC time.
        private readonly IClock _clock;


        // PROPERTIES ////////////////////////////////
        // 'DbSet' class is used to represent a collection of entities in the context.
        // What the following line does is that it creates a DbSet for the 'Person' entity.
        // 'DbSet' is something like a table in a database.
        // So, this line is saying that we have a table of 'Person' entities in our database context.
        // So, we can access the Person model in the application using the People property.
        public DbSet<Person> People { get; set; }


        // CONSTRUCTORS //////////////////////////////
        // Constructor that takes DbContextOptions and IClock as parameters.
        // In fact, the input parameters are provided by Dependency Injection (DI) in ASP.NET Core.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IClock clock)
        : base(options)
        {
            _clock = clock;
        }


        // METHODS ///////////////////////////////////
        // Apply auditing before saving changes to the database.
        // Auditing means keeping track of who created or modified a record and when.
        // The return type is 'int' meaning the number of state entries written to the database.
        public override int SaveChanges()
        {
            ApplyAuditing();
            return base.SaveChanges();
        }


        // Asynchronous version of the above SaveChanges method.
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditing();
            return base.SaveChangesAsync(cancellationToken);
        }


        // Private method to apply auditing information.
        // Auditing means keeping track of who created or modified a record and when.
        // This method sets the CreatedAt and UpdatedAt properties of Person entities.
        private void ApplyAuditing()
        {
            var now = _clock.GetCurrentInstant();

            // The 'ChangeTracker' property is used to get information about the entities being tracked by the context.
            // The 'Entries()' method returns all the tracked entities as an IEnumerable collection.
            // The 'Where()' method filters the entries to include only those that are of type 'Person' and are either being added or modified.
            // 'Where' is a LINQ method that filters a collection based on a condition.
            foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is Person && (e.State == EntityState.Added || e.State == EntityState.Modified)))
            {
                var entity = (Person)entry.Entity;
                if (entry.State == EntityState.Added)
                    entity.CreatedAt = now;
                entity.UpdatedAt = now;
            }
        }


        // Override the OnModelCreating method to configure the model.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set the default collation for the database.
            // 'Collation' means the set of rules that determine how data is sorted and compared.
            // In this case, we are using the "SQL_Latin1_General_CP1_CI_AS" collation, which means
            // case-insensitive (CI) and accent-sensitive (AS) comparisons for Latin1 characters.
            // Latin1 is a character encoding standard that includes characters for Western European languages.
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

            base.OnModelCreating(modelBuilder);
        }
    }
}