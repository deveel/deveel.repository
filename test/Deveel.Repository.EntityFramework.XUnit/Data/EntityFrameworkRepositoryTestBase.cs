using System.ComponentModel.DataAnnotations;

using Bogus;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public class EntityFrameworkRepositoryTestBase : IAsyncLifetime {
        private readonly IServiceProvider serviceProvider;
        private readonly SqliteConnection sqliteConnection;

        protected EntityFrameworkRepositoryTestBase(SqliteConnection sqliteConnection) {
            this.sqliteConnection = sqliteConnection;

            var services = new ServiceCollection();
            AddRepository(services);

            serviceProvider = services.BuildServiceProvider();

            PersonFaker = new Faker<PersonEntity>()
                .RuleFor(x => x.FirstName, f => f.Name.FirstName())
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.BirthDate, f => f.Date.Past(20));
            
        }

        protected Faker<PersonEntity> PersonFaker { get; }

        protected PersonEntity GeneratePerson() => PersonFaker.Generate();

        protected IList<PersonEntity> GeneratePersons(int count) => PersonFaker.Generate(count);

        protected virtual void AddRepository(IServiceCollection services) {
            services
                .AddDbContext<TestDbContext>(builder => {
                    builder.UseSqlite(sqliteConnection);
                })
                .AddEntityRepository<PersonEntity>()
                .AddEntityFacadeRepository<PersonEntity, IPerson>()
                .AddRepositoryController();
        }


        public Task DisposeAsync() {
            throw new NotImplementedException();
        }

        public Task InitializeAsync() {
            throw new NotImplementedException();
        }

        protected class TestDbContext : DbContext {
            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<PersonEntity>()
                    .HasMany(x => x.Relationships)
                    .WithOne(x => x.Person)
                    .HasForeignKey(x => x.PersonId);

                modelBuilder.Entity<PersonRelationship>();

                base.OnModelCreating(modelBuilder);
            }
        }

        protected class PersonEntity : IPerson {
            [Key]
            public Guid Id { get; set; }


            string? IDataEntity.Id => Id.ToString();

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateTime? BirthDate { get; set; }

            public string? Description { get; set; }

            [Version]
            public string Version { get; set; }

            public IList<PersonRelationship> Relationships { get; set; }

            IEnumerable<IRelationship> IPerson.Relationships => Relationships;
        }

        protected class PersonRelationship : IRelationship {
            [Key]
            public Guid Id { get; set; }

            public Guid PersonId { get; set; }

            public PersonEntity Person { get; set; }

            public string Type { get; set; }

            public string FullName { get; set; }
        }

        protected interface IPerson : IDataEntity {
            public string FirstName { get; }

            public string LastName { get; }

            public DateTime? BirthDate { get; }

            public string? Description { get; }

            IEnumerable<IRelationship> Relationships { get; }
        }

        protected interface IRelationship {
            string Type { get; }

            string FullName { get; }
        }
    }
}
