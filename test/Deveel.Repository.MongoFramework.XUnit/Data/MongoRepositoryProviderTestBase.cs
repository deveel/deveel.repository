using Bogus;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using MongoFramework;

namespace Deveel.Data {
	[Collection("Mongo Single Database")]

	public abstract class MongoRepositoryProviderTestBase : IAsyncLifetime {
		private MongoFrameworkTestFixture mongo;
		private readonly IServiceProvider serviceProvider;

		protected MongoRepositoryProviderTestBase(MongoFrameworkTestFixture mongo) {
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

		protected MongoRepositoryProvider<MongoPerson, TenantInfo> MongoRepositoryProvider => serviceProvider.GetRequiredService<MongoRepositoryProvider<MongoPerson, TenantInfo>>();

		protected MongoRepository<MongoDbTenantContext, MongoPerson> MongoRepository => MongoRepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

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
					}) ;
				});

			services
				.AddMongoContext(builder => {
					builder.UseTenantConnection();
					builder.AddRepository<MongoPerson>()
						.WithDefaultProvider()
						.WithFacade<IPerson>()
						.WithDefaultFacadeProvider<IPerson>();
				})
				.AddRepositoryController();
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

		protected virtual Task SeedAsync(MongoRepository<MongoDbTenantContext, MongoPerson> repository) {
			return Task.CompletedTask;
		}

		[MultiTenant]
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
	}
}
