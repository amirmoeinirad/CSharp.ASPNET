using NodaTime;

namespace MyApp.Web.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public Instant CreatedAt { get; set; }   // NodaTime Instant
        public Instant? UpdatedAt { get; set; }
    }
}