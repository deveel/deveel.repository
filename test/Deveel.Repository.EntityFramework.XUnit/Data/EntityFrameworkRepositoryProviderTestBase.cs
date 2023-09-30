using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Bogus;

using Deveel.Logging;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Deveel.Data {
    [Collection(nameof(SqlConnectionCollection))]
    public abstract class EntityFrameworkRepositoryProviderTestBase : IAsyncLifetime {
        private readonly IServiceProvider serviceProvider;
        private readonly SqliteConnection sqliteConnection;

        protected EntityFrameworkRepositoryProviderTestBase(SqlTestConnection testCollection, ITestOutputHelper outputHelper) {
            this.sqliteConnection = testCollection.Connection;

            var services = new ServiceCollection();

            if (outputHelper != null)
                services.AddLogging(logging => logging.AddXUnit(outputHelper).SetMinimumLevel(LogLevel.Trace));

            TenantId = Guid.NewGuid().ToString();

            services.AddMultiTenant<TenantInfo>()
                .WithInMemoryStore(options => {
                    options.Tenants.Add(new TenantInfo {
                        Id = TenantId,
                        Identifier = "test",
                        Name = "Test Tenant",
                        ConnectionString = sqliteConnection.ConnectionString
                    });
                });

            AddRepository(services);

            serviceProvider = services.BuildServiceProvider();

            var relTypes = new string[] { "father", "mother", "brother", "sister", "partner" };

            var relationshipFaker = new Faker<PersonRelationship>()
                .RuleFor(x => x.FullName, f => f.Name.FullName())
                .RuleFor(x => x.Type, f => f.PickRandom(relTypes));

            PersonFaker = new Faker<PersonEntity>()
                .RuleFor(x => x.FirstName, f => f.Name.FirstName())
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.BirthDate, f => f.Date.Past(20))
                .RuleFor(x => x.Relationships, (f, p) => {
                    return relationshipFaker.FinishWith((f2, x) => { x.Person = p; }).Generate(3);
                });
        }

        protected Faker<PersonEntity> PersonFaker { get; }

        protected string TenantId { get; }

        protected EntityRepositoryProvider<PersonEntity, TestDbContext> EntityRepositoryProvider 
            => serviceProvider.GetRequiredService<EntityRepositoryProvider<PersonEntity, TestDbContext>>();

        protected EntityRepository<PersonEntity> EntityRepository => EntityRepositoryProvider.GetRepository(TenantId);

        protected IRepositoryProvider<PersonEntity> RepositoryProvider 
            => serviceProvider.GetRequiredService<IRepositoryProvider<PersonEntity>>();

        protected IRepository<PersonEntity> Repository => RepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

        protected PersonEntity GeneratePerson() => PersonFaker.Generate();

        protected IList<PersonEntity> GeneratePersons(int count) => PersonFaker.Generate(count);

        protected virtual void AddRepository(IServiceCollection services) {
            services
                .AddDbContext<DbContext, TestDbContext>(builder => {
                    builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
                    builder.UseSqlite(sqliteConnection);
                })
                .AddEntityRepositoryProvider<TestDbContext, PersonEntity>()
                .AddEntityFacadeRepositoryProvider<TestDbContext, PersonEntity, IPerson>();
        }

        protected virtual Task SeedAsync(EntityRepository<PersonEntity> repository) {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync() {
            var dbContext = serviceProvider.GetRequiredService<DbContext>();
            await dbContext.Database.EnsureDeletedAsync();

            await dbContext.DisposeAsync();

            await sqliteConnection.CloseAsync();
        }

        public async Task InitializeAsync() {
            if (sqliteConnection.State != System.Data.ConnectionState.Open)
                await sqliteConnection.OpenAsync();

            var dbContext = serviceProvider.GetRequiredService<DbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            await SeedAsync(EntityRepository);
        }

        protected class TestDbContext : MultiTenantDbContext {
            public TestDbContext(ITenantInfo tenantInfo, DbContextOptions<TestDbContext> options) : base(tenantInfo, options) {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<PersonEntity>()
                    .HasMany(x => x.Relationships)
                    .WithOne(x => x.Person)
                    .HasForeignKey(x => x.PersonId);

                modelBuilder.Entity<PersonEntity>()
                    .IsMultiTenant();

                modelBuilder.Entity<PersonRelationship>();

                base.OnModelCreating(modelBuilder);
            }
        }

        [Table("people")]
        protected class PersonEntity : IPerson {
            [Key]
            public Guid Id { get; set; }

            string IPerson.Id => Id.ToString();

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateTime? BirthDate { get; set; }

            public string? Description { get; set; }

            public string TenantId { get; set; }

            public IList<PersonRelationship>? Relationships { get; set; }

            IEnumerable<IRelationship> IPerson.Relationships => Relationships;
        }

        [Table("person_relationships")]
        protected class PersonRelationship : IRelationship {
            [Key]
            public Guid Id { get; set; }

            public Guid PersonId { get; set; }

            public PersonEntity Person { get; set; }

            public string Type { get; set; }

            public string FullName { get; set; }
        }

        protected interface IPerson {
            string Id { get; }

            string FirstName { get; }

            string LastName { get; }

            DateTime? BirthDate { get; }

            string? Description { get; }

            IEnumerable<IRelationship> Relationships { get; }
        }

        protected interface IRelationship {
            string Type { get; }

            string FullName { get; }
        }
    }
}
