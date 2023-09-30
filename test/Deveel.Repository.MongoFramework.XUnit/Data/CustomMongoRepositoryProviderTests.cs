using Bogus;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using MongoFramework;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
    public abstract class CustomMongoRepositoryProviderTests : IAsyncLifetime {
        private MongoSingleDatabase mongo;
        private readonly IServiceProvider serviceProvider;

        protected CustomMongoRepositoryProviderTests(MongoSingleDatabase mongo) {
            this.mongo = mongo;

            var services = new ServiceCollection();
            AddRepositoryProvider(services);

            serviceProvider = services.BuildServiceProvider();

            PersonFaker = new MongoTenantPersonFaker(TenantId);
        }

        protected Faker<MongoTenantPerson> PersonFaker { get; }

        protected string TenantId { get; } = Guid.NewGuid().ToString("N");

        protected string ConnectionString => mongo.ConnectionString;

        protected MongoTenantRepositoryProvider<PersonsDbContext, MongoTenantPerson, TenantInfo> MongoRepositoryProvider => serviceProvider.GetRequiredService<MongoTenantRepositoryProvider<PersonsDbContext, MongoTenantPerson, TenantInfo>>();

        protected MongoRepository<PersonsDbContext, MongoTenantPerson> MongoRepository => MongoRepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

        protected IRepositoryProvider<MongoTenantPerson> RepositoryProvider => serviceProvider.GetRequiredService<IRepositoryProvider<MongoTenantPerson>>();

        protected IRepository<MongoTenantPerson> Repository => RepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

        protected IFilterableRepository<MongoTenantPerson> FilterableRepository => (IFilterableRepository<MongoTenantPerson>)Repository;

        protected IPageableRepository<MongoTenantPerson> PageableRepository => (IPageableRepository<MongoTenantPerson>)Repository;

        protected IRepositoryProvider<IPerson> FacadeRepositoryProvider => serviceProvider.GetRequiredService<IRepositoryProvider<IPerson>>();

        protected IRepository<IPerson> FacadeRepository => FacadeRepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

        protected IPageableRepository<IPerson> FacadePageableRepository => (IPageableRepository<IPerson>)FacadeRepository;

        protected IFilterableRepository<IPerson> FilterableFacadeRepository => (IFilterableRepository<IPerson>)FacadeRepository;

        protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

        protected MongoTenantPerson GeneratePerson() => PersonFaker.Generate();

        protected IList<MongoTenantPerson> GeneratePersons(int count)
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
            builder.AddRepository<MongoTenantPerson>()
                .OfType<PersonRepository>()
                .WithTenantProvider<PersonRepositoryProvider>()
                .WithFacade<IPerson>()
                .WithTenantFacadeProvider<IPerson, PersonRepositoryProvider>();
        }

        public virtual async Task InitializeAsync() {
            var controller = serviceProvider.GetRequiredService<IRepositoryController>();
            await controller.CreateTenantRepositoryAsync<MongoTenantPerson>(TenantId);

            var repository = await MongoRepositoryProvider.GetRepositoryAsync(TenantId);

            //await repository.CreateAsync();

            await SeedAsync(repository);
        }

        public virtual async Task DisposeAsync() {
            var controller = serviceProvider.GetRequiredService<IRepositoryController>();
            await controller.DropTenantRepositoryAsync<MongoTenantPerson>(TenantId);

            //var repository = MongoRepositoryProvider.GetRepository(TenantId);
            //await repository.DropAsync();
        }

        protected virtual Task SeedAsync(IRepository<MongoTenantPerson> repository) {
            return Task.CompletedTask;
        }

        //[MultiTenant, Entity]
        //protected class MongoPerson : IPerson, IHaveTenantId {
        //    [BsonId]
        //    public ObjectId Id { get; set; }

        //    string? IPerson.Id => Id.ToEntityId();

        //    public string FirstName { get; set; }

        //    public string LastName { get; set; }

        //    public DateTime? BirthDate { get; set; }

        //    public string? Description { get; set; }

        //    public string TenantId { get; set; }
        //}

        //protected interface IPerson {
        //    string? Id { get; }

        //    string FirstName { get; }

        //    string LastName { get; }

        //    DateTime? BirthDate { get; }

        //    string? Description { get; }
        //}

        protected class PersonRepositoryProvider : MongoTenantRepositoryProvider<PersonsDbContext, MongoTenantPerson, IPerson, TenantInfo> {
            public PersonRepositoryProvider(IEnumerable<IMultiTenantStore<TenantInfo>> stores, ISystemTime? systemTime = null, ILoggerFactory? loggerFactory = null) 
				:base(stores, systemTime, loggerFactory) {
            }

            protected override PersonsDbContext CreateContext(IMongoDbConnection connection, IMultiTenantContext<TenantInfo> tenantContext) {
                return new PersonsDbContext(connection.ForContext<PersonsDbContext>(), tenantContext.TenantInfo.Id);
            }

            protected override MongoRepository<PersonsDbContext, MongoTenantPerson, IPerson> CreateRepository(PersonsDbContext context) {
                var logger = LoggerFactory.CreateLogger<PersonRepository>();
                return new PersonRepository(context, logger);
            }
        }

        protected class PersonRepository : MongoRepository<PersonsDbContext, MongoTenantPerson, IPerson> {
            public PersonRepository(PersonsDbContext context, ILogger<PersonRepository>? logger = null) : base(context, null, logger) {
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
