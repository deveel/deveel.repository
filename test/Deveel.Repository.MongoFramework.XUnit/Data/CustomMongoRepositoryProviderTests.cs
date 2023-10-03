using Bogus;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoFramework;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class CustomMongoRepositoryProviderTests : MongoRepositoryTestSuite<MongoTenantPerson> {
        public CustomMongoRepositoryProviderTests(MongoSingleDatabase mongo, ITestOutputHelper outputHelper)
			: base(mongo, outputHelper) {
            PersonFaker = new MongoTenantPersonFaker(TenantId);
        }

        protected override Faker<MongoTenantPerson> PersonFaker { get; }

        protected string TenantId { get; } = Guid.NewGuid().ToString("N");

        protected IRepositoryProvider<MongoTenantPerson> RepositoryProvider => Services.GetRequiredService<IRepositoryProvider<MongoTenantPerson>>();

        protected override IRepository<MongoTenantPerson> Repository => RepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

		protected override void ConfigureServices(IServiceCollection services) {
			AddRepositoryProvider(services);

			base.ConfigureServices(services);
		}

		protected virtual void AddRepositoryProvider(IServiceCollection services) {
            services.AddMultiTenant<TenantInfo>()
                .WithInMemoryStore(config => {
                    config.Tenants.Add(new TenantInfo {
                        Id = TenantId,
                        Identifier = "test",
                        Name = "Test Tenant",
                        ConnectionString = ConnectionString
                    });
                });

            AddMongoDbContext(services);

            services.AddRepositoryController();
        }

        protected virtual void AddMongoDbContext(IServiceCollection services) {
            services.AddMongoDbContext<PersonsDbContext>((tenant, builder) => builder.UseConnection(tenant.ConnectionString!));
            AddRepository(services);
        }

        protected virtual void AddRepository(IServiceCollection services) {
			services.AddRepository<PersonRepository>();
			services.AddRepositoryProvider<PersonRepositoryProvider>();			
        }

        protected override async Task InitializeAsync() {
            var controller = Services.GetRequiredService<IRepositoryController>();
            await controller.CreateTenantRepositoryAsync<MongoTenantPerson>(TenantId);

            var repository = await RepositoryProvider.GetRepositoryAsync(TenantId);

            //await repository.CreateAsync();

            await SeedAsync(repository);
        }

        protected override async Task DisposeAsync() {
            var controller = Services.GetRequiredService<IRepositoryController>();
            await controller.DropTenantRepositoryAsync<MongoTenantPerson>(TenantId);
        }

		[Fact]
		public void ValidateRepositoryTypes() {
			Assert.IsType<PersonRepository>(Repository);
			Assert.IsType<PersonRepositoryProvider>(RepositoryProvider);
		}

		protected class PersonRepositoryProvider : MongoRepositoryProvider<PersonsDbContext, MongoTenantPerson, TenantInfo> {
            public PersonRepositoryProvider(IEnumerable<IMultiTenantStore<TenantInfo>> stores, ISystemTime? systemTime = null, ILoggerFactory? loggerFactory = null) 
				:base(stores, systemTime, loggerFactory) {
            }

            protected override PersonsDbContext CreateContext(IMongoDbConnection connection, TenantInfo tenantInfo) {
                return new PersonsDbContext(connection.ForContext<PersonsDbContext>(), tenantInfo?.Id ?? throw new InvalidOperationException());
            }

            protected override MongoRepository<MongoTenantPerson> CreateRepository(PersonsDbContext context) {
                var logger = LoggerFactory.CreateLogger<PersonRepository>();
                return new PersonRepository(context, logger);
            }
        }

        protected class PersonRepository : MongoRepository<MongoTenantPerson> {
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
