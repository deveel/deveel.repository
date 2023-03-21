using System.ComponentModel.DataAnnotations;

using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;

namespace Deveel.Data {
	[Collection("Mongo Single Database")]
	public abstract class MongoFrameworkRepositoryTestBase : IAsyncLifetime {
		private MongoFrameworkTestFixture mongo;
		private readonly IServiceProvider serviceProvider;

		protected MongoFrameworkRepositoryTestBase(MongoFrameworkTestFixture mongo) {
			this.mongo = mongo;

			var services = new ServiceCollection();
			AddRepository(services);

			serviceProvider = services.BuildServiceProvider();

			PersonFaker = new Faker<MongoPerson>()
				.RuleFor(x => x.FirstName, f => f.Name.FirstName())
				.RuleFor(x => x.LastName, f => f.Name.LastName())
				.RuleFor(x => x.BirthDate, f => f.Date.Past(20));
		}

		protected string ConnectionString => mongo.ConnectionString;

		protected MongoRepository<MongoPerson> MongoRepository => serviceProvider.GetRequiredService<MongoRepository<MongoPerson>>();

		protected IRepository<MongoPerson> Repository => serviceProvider.GetRequiredService<IRepository<MongoPerson>>();

		protected IFilterableRepository<MongoPerson> FilterableRepository => Repository as IFilterableRepository<MongoPerson>;

		protected IPageableRepository<MongoPerson> PageableRepository => Repository as IPageableRepository<MongoPerson>;

		protected IRepository<IPerson> FacadeRepository => serviceProvider.GetRequiredService<IRepository<IPerson>>();

        protected IFilterableRepository<IPerson> FilterableFacadeRepository => FacadeRepository as IFilterableRepository<IPerson>;

        protected IPageableRepository<IPerson> FacadePageableRepository => FacadeRepository as IPageableRepository<IPerson>;

		protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

		protected Faker<MongoPerson> PersonFaker { get; }

		protected MongoPerson GeneratePerson() => PersonFaker.Generate();

		protected IList<MongoPerson> GeneratePersons(int count) => PersonFaker.Generate(count);

		protected virtual void AddRepository(IServiceCollection services) {
			services
				.AddMongoContext(options => { 
					options.ConnectionString = ConnectionString;
					options.DatabaseName = "test_db";
				})
				.AddMongoRepository<MongoPerson>()
				.AddMongoFacadeRepository<MongoPerson, IPerson>()
				.AddRepositoryController();
		}

		protected virtual Task SeedAsync(MongoRepository<MongoPerson> repository) {
			return Task.CompletedTask;
		}

		public virtual async Task InitializeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
			await controller.CreateRepositoryAsync<MongoPerson>();

			await SeedAsync(MongoRepository);
		}

		public virtual async Task DisposeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
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
