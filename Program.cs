using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDatabaseFunctions
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var db = new TestDbContext();
            
            // database setup
            await db.Database.EnsureCreatedAsync();
            await db.Database.MigrateAsync();
            await db.Database.ExecuteSqlRawAsync("Truncate Table People");
            await db.Database.ExecuteSqlRawAsync("IF OBJECT_ID('dbo.FortyTwo') IS NOT NULL DROP FUNCTION FortyTwo");
            await db.Database.ExecuteSqlRawAsync("CREATE FUNCTION FortyTwo() RETURNS int AS BEGIN return 42; END");
            // end database setup
            
            var john = new Person
            {
                Maybe = DateTime.Now.ToShortDateString(),
                Name = "John Doe",
                Json = @"{ ""hello"" : ""world"" }"
            };

            await db.People.AddAsync(john);
            await db.SaveChangesAsync();

            var joSchmoes =
            await db.People
                .Where(p => EF.Functions.Like(p.Name, "jo%"))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    // EF Core packaged Function
                    hasDate = EF.Functions.IsDate(p.Maybe),
                    // SQL Server JSON_VALUE Function
                    hello = Json.Value(p.Json, "$.hello"),
                    // Custom Function
                    meaningOfLife = AnswersToTheUniverse.What()
                })
                .ToListAsync();
        }
    }

    public class TestDbContext : DbContext
    {
        private static readonly ILoggerFactory DemoLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });
        public DbSet<Person> People { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(DemoLoggerFactory)
                // update the connection string to your database
                .UseSqlServer("server=localhost,11433;database=test;user=sa;password=Pass123!;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Register our Functions
            modelBuilder
                .UseCustomDbFunctions();
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Json { get; set; }
        
        public string Maybe { get; set; }
    }
}