using Bogus;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]

	public abstract class MongoRepositoryProviderTestBase : IAsyncLifetime {
		private MongoSingleDatabase mongo;
		private readonly IServiceProvider serviceProvider;

		protected MongoRepositoryProviderTestBase(MongoSingleDatabase mongo) {
			this.mongo = mongo;

			var services = new ServiceCollection();
			AddRepositoryProvider(services);

			serviceProvider = services.BuildServiceProvider();

			PersonFaker = new MongoTenantPersonFaker(TenantId);
		}

		protected virtual string DatabaseName { get; } = "test_db";

		protected IMongoCollection<MongoTenantPerson> MongoCollection => new MongoClient(mongo.ConnectionString)
			.GetDatabase(DatabaseName)
			.GetCollection<MongoTenantPerson>("persons");

		protected async Task<MongoTenantPerson?> FindPerson(ObjectId id) {
			var collection = MongoCollection;
			var result = await collection.FindAsync(x => x.Id == id);

			return await result.FirstOrDefaultAsync();
		}

		protected Faker<MongoTenantPerson> PersonFaker { get; }

		protected string TenantId { get; } = Guid.NewGuid().ToString("N");

		protected string ConnectionString => mongo.ConnectionString;

		protected MongoTenantRepositoryProvider<MongoDbTenantContext, MongoTenantPerson, TenantInfo> MongoRepositoryProvider 
			=> serviceProvider.GetRequiredService<MongoTenantRepositoryProvider<MongoDbTenantContext, MongoTenantPerson, TenantInfo>>();

		protected MongoRepository<MongoDbTenantContext, MongoTenantPerson> MongoRepository => 
			MongoRepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

		protected IRepositoryProvider<MongoTenantPerson> RepositoryProvider => 
			serviceProvider.GetRequiredService<IRepositoryProvider<MongoTenantPerson>>();

		protected IRepository<MongoTenantPerson> Repository => RepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

		protected IFilterableRepository<MongoTenantPerson> FilterableRepository => (IFilterableRepository<MongoTenantPerson>)Repository;

		protected IPageableRepository<MongoTenantPerson> PageableRepository => (IPageableRepository<MongoTenantPerson>)Repository;

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
						ConnectionString = mongo.SetDatabase(DatabaseName)
					}) ;
				});

			AddMongoDbContext(services);
			
			services.AddRepositoryController();
		}

		protected virtual void AddMongoDbContext(IServiceCollection services) {
			var builder = services.AddMongoTenantContext();
			AddRepository(builder);
		}

		protected virtual void AddRepository<TContext>(MongoDbContextBuilder<TContext> builder)
			where TContext : class, IMongoDbContext {
            builder.UseTenantConnection();
			builder.AddRepository<MongoTenantPerson>()
				.WithDefaultTenantProvider();
        }

        public virtual async Task InitializeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
			await controller.CreateTenantRepositoryAsync<MongoTenantPerson>(TenantId);

			var repository = await MongoRepositoryProvider.GetRepositoryAsync(TenantId);
			
			await SeedAsync(repository);
		}

		public virtual async Task DisposeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
			await controller.DropTenantRepositoryAsync<MongoTenantPerson>(TenantId);
		}

		protected virtual Task SeedAsync(IRepository<MongoTenantPerson> repository) {
			return Task.CompletedTask;
		}
	}
}
