using System.ComponentModel.DataAnnotations;

using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data {
	[Collection("Mongo Single Database")]
	public abstract class MongoFrameworkRepositoryTestBase : IAsyncLifetime {
		private MongoFrameworkTestFixture mongo;

		protected MongoFrameworkRepositoryTestBase(MongoFrameworkTestFixture mongo) {
			this.mongo = mongo;

			var services = new ServiceCollection();
			AddRepository(services);

			Services = services.BuildServiceProvider();

			PersonFaker = new Faker<MongoPerson>()
				.RuleFor(x => x.FirstName, f => f.Name.FirstName())
				.RuleFor(x => x.LastName, f => f.Name.LastName())
				.RuleFor(x => x.BirthDate, f => f.Date.Past(20));
		}

		protected string ConnectionString => mongo.ConnectionString;

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

		public virtual async Task InitializeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.CreateRepositoryAsync<MongoPerson>();

			await SeedAsync(MongoRepository);
		}

		public virtual async Task DisposeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.DropRepositoryAsync<MongoPerson>();
		}

		protected class MongoPerson : IPerson {
			[Key]
			public ObjectId Id { get; set; }


			string? IPerson.Id => Id.ToEntityId();

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public DateTime? BirthDate { get; set; }

			public string? Description { get; set; }

			[Version]
			public string Version { get; set; }
		}

		protected interface IPerson {
			string? Id { get; }

			public string FirstName { get; }

			public string LastName { get; }

			public DateTime? BirthDate { get; }

			public string? Description { get; }
		}
	}
}
