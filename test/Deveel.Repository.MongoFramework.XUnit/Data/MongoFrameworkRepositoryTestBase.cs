using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public abstract class MongoFrameworkRepositoryTestBase : IAsyncLifetime {
		private MongoSingleDatabase mongo;

		protected MongoFrameworkRepositoryTestBase(MongoSingleDatabase mongo) {
			this.mongo = mongo;

			var services = new ServiceCollection();
			AddRepository(services);

			Services = services.BuildServiceProvider();

			PersonFaker = new MongoPersonFaker();
		}

		protected string ConnectionString => mongo.ConnectionString;

		protected IMongoCollection<MongoPerson> MongoCollection => new MongoClient(mongo.ConnectionString)
			.GetDatabase("testdb")
			.GetCollection<MongoPerson>("persons");


		protected IServiceProvider Services { get; }

		protected MongoRepository<MongoDbContext, MongoPerson> MongoRepository => Services.GetRequiredService<MongoRepository<MongoDbContext, MongoPerson>>();

		protected IRepository<MongoPerson> Repository => Services.GetRequiredService<IRepository<MongoPerson>>();

		protected IFilterableRepository<MongoPerson> FilterableRepository => (IFilterableRepository<MongoPerson>)Repository;

		protected IPageableRepository<MongoPerson> PageableRepository => (IPageableRepository<MongoPerson>)Repository;

		protected IRepository<IPerson> FacadeRepository => Services.GetRequiredService<IRepository<IPerson>>();

		protected IFilterableRepository<IPerson> FilterableFacadeRepository => (IFilterableRepository<IPerson>)FacadeRepository;

		protected IPageableRepository<IPerson> FacadePageableRepository => (IPageableRepository<IPerson>)FacadeRepository;

		protected IDataTransactionFactory TransactionFactory => Services.GetRequiredService<IDataTransactionFactory>();

		protected Faker<MongoPerson> PersonFaker { get; }

		protected MongoPerson GeneratePerson() => PersonFaker.Generate();

		protected IList<MongoPerson> GeneratePersons(int count) => PersonFaker.Generate(count);

		protected virtual void AddRepository(IServiceCollection services) {
			services
				.AddMongoContext(builder => { 
					builder.UseConnection(mongo.SetDatabase("testdb"));
					AddRepository(builder);
				})
				.AddRepositoryController();
		}

		protected virtual void AddRepository(MongoDbContextBuilder<MongoDbContext> builder) {
            builder.AddRepository<MongoPerson>().WithFacade<IPerson>();
        }

		protected virtual Task SeedAsync(MongoRepository<MongoDbContext, MongoPerson> repository) {
			return Task.CompletedTask;
		}

		protected async Task<MongoPerson?> FindPerson(ObjectId id) {
			var collection = MongoCollection;
			var result = await collection.FindAsync(x => x.Id == id);

			return await result.FirstOrDefaultAsync();
		}


		public virtual async Task InitializeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.CreateRepositoryAsync<MongoPerson>();

			await SeedAsync(MongoRepository);
		}

		public virtual async Task DisposeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.DropRepositoryAsync<MongoPerson>();
		}
	}
}
