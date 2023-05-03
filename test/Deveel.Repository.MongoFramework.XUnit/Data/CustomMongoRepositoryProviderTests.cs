using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bogus;
using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;

using MongoDB.Bson;

using MongoFramework;
using Deveel.Data.Entities;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
    [Collection("Mongo Single Database")]
    public abstract class CustomMongoRepositoryProviderTests : IAsyncLifetime {
        private MongoFrameworkTestFixture mongo;
        private readonly IServiceProvider serviceProvider;

        protected CustomMongoRepositoryProviderTests(MongoFrameworkTestFixture mongo) {
            this.mongo = mongo;

            var services = new ServiceCollection();
            AddRepositoryProvider(services);

            serviceProvider = services.BuildServiceProvider();

            PersonFaker = new Faker<MongoPerson>()
                .RuleFor(x => x.FirstName, f => f.Name.FirstName())
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.BirthDate, f => f.Date.Past(20));
        }

        protected Faker<MongoPerson> PersonFaker { get; }

        protected string TenantId { get; } = Guid.NewGuid().ToString("N");

        protected string ConnectionString => mongo.ConnectionString;

        protected MongoTenantRepositoryProvider<PersonsDbContext, MongoPerson, TenantInfo> MongoRepositoryProvider => serviceProvider.GetRequiredService<MongoTenantRepositoryProvider<PersonsDbContext, MongoPerson, TenantInfo>>();

        protected MongoRepository<PersonsDbContext, MongoPerson> MongoRepository => MongoRepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

        protected IRepositoryProvider<MongoPerson> RepositoryProvider => serviceProvider.GetRequiredService<IRepositoryProvider<MongoPerson>>();

        protected IRepository<MongoPerson> Repository => RepositoryProvider.GetRepository(TenantId);

        protected IFilterableRepository<MongoPerson> FilterableRepository => (IFilterableRepository<MongoPerson>)Repository;

        protected IPageableRepository<MongoPerson> PageableRepository => (IPageableRepository<MongoPerson>)Repository;

        protected IRepositoryProvider<IPerson> FacadeRepositoryProvider => serviceProvider.GetRequiredService<IRepositoryProvider<IPerson>>();

        protected IRepository<IPerson> FacadeRepository => FacadeRepositoryProvider.GetRepository(TenantId);

        protected IPageableRepository<IPerson> FacadePageableRepository => (IPageableRepository<IPerson>)FacadeRepository;

        protected IFilterableRepository<IPerson> FilterableFacadeRepository => (IFilterableRepository<IPerson>)FacadeRepository;

        protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

        protected MongoPerson GeneratePerson() => PersonFaker.Generate();

        protected IList<MongoPerson> GeneratePersons(int count)
            => PersonFaker.Generate(count);

        protected virtual void AddRepositoryProvider(IServiceCollection services) {
            services.AddMultiTenant<TenantInfo>()
                .WithInMemoryStore(config => {
                    config.Tenants.Add(new TenantInfo {
                        Id = TenantId,
                        Identifier = "test",
                        Name = "Test Tenant",
                        ConnectionString = mongo.SetDatabase("test_db")
                    });
                });

            AddMongoDbContext(services);

            services.AddRepositoryController();
        }

        protected virtual void AddMongoDbContext(IServiceCollection services) {
            var builder = services.AddMongoTenantContext<PersonsDbContext>();
            AddRepository(builder);
        }

        protected virtual void AddRepository(MongoDbContextBuilder<PersonsDbContext> builder) {
            builder.UseTenantConnection();
            builder.AddRepository<MongoPerson>()
                .OfType<PersonRepository>()
                .WithTenantProvider<PersonRepositoryProvider>()
                .WithFacade<IPerson>()
                .WithTenantFacadeProvider<IPerson, PersonRepositoryProvider>();
        }

        public virtual async Task InitializeAsync() {
            var controller = serviceProvider.GetRequiredService<IRepositoryController>();
            await controller.CreateTenantRepositoryAsync<MongoPerson>(TenantId);

            var repository = await MongoRepositoryProvider.GetRepositoryAsync(TenantId);

            //await repository.CreateAsync();

            await SeedAsync(repository);
        }

        public virtual async Task DisposeAsync() {
            var controller = serviceProvider.GetRequiredService<IRepositoryController>();
            await controller.DropTenantRepositoryAsync<MongoPerson>(TenantId);

            //var repository = MongoRepositoryProvider.GetRepository(TenantId);
            //await repository.DropAsync();
        }

        protected virtual Task SeedAsync(IRepository<MongoPerson> repository) {
            return Task.CompletedTask;
        }

        [MultiTenant, Entity]
        protected class MongoPerson : IPerson, IHaveTenantId {
            [BsonId]
            public ObjectId Id { get; set; }

            string? IPerson.Id => Id.ToEntityId();

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateTime? BirthDate { get; set; }

            public string? Description { get; set; }

            public string TenantId { get; set; }
        }

        protected interface IPerson {
            string? Id { get; }

            string FirstName { get; }

            string LastName { get; }

            DateTime? BirthDate { get; }

            string? Description { get; }
        }

        protected class PersonRepositoryProvider : MongoTenantRepositoryProvider<PersonsDbContext, MongoPerson, IPerson, TenantInfo> {
            public PersonRepositoryProvider(IEnumerable<IMultiTenantStore<TenantInfo>> stores, ILoggerFactory? loggerFactory = null)
                : base(stores, loggerFactory) {
            }

            protected override PersonsDbContext CreateContext(IMongoDbConnection connection, IMultiTenantContext<TenantInfo> tenantContext) {
                return new PersonsDbContext(connection.ForContext<PersonsDbContext>(), tenantContext.TenantInfo.Id);
            }

            protected override MongoRepository<PersonsDbContext, MongoPerson, IPerson> CreateRepository(PersonsDbContext context) {
                var logger = LoggerFactory.CreateLogger<PersonRepository>();
                return new PersonRepository(context, logger);
            }
        }

        protected class PersonRepository : MongoRepository<PersonsDbContext, MongoPerson, IPerson> {
            public PersonRepository(PersonsDbContext context, ILogger<PersonRepository>? logger = null) : base(context, logger) {
            }
        }

        protected class PersonsDbContext : MongoDbTenantContext {
            public PersonsDbContext(IMongoDbConnection<PersonsDbContext> connection, string tenantId)
                : base(connection, tenantId) {
            }

            protected override void OnConfigureMapping(MappingBuilder mappingBuilder) {
                mappingBuilder.Entity<MongoPerson>();
            }
        }

    }
}
