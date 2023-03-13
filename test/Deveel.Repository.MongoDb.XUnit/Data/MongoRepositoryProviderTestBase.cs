using System;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Bogus;

namespace Deveel.Data {
	[Collection("Mongo Single Database")]

	public abstract class MongoRepositoryProviderTestBase : IAsyncLifetime {
		private MongoDbTestFixture mongo;
		private readonly IServiceProvider serviceProvider;

		protected MongoRepositoryProviderTestBase(MongoDbTestFixture mongo) {
			this.mongo = mongo;

			var services = new ServiceCollection();
			AddRepositoryProvider(services);

			services.AddMongoTransactionFactory();

			serviceProvider = services.BuildServiceProvider();

			PersonFaker = new Faker<MongoPerson>()
				.RuleFor(x => x.FirstName, f => f.Name.FirstName())
				.RuleFor(x => x.LastName, f => f.Name.LastName())
				.RuleFor(x => x.BirthDate, f => f.Date.Past(20));
		}

		protected string TenantId { get; } = Guid.NewGuid().ToString("N");

		protected string ConnectionString => mongo.ConnectionString;

		protected MongoRepositoryProvider<MongoPerson> MongoRepositoryProvider => serviceProvider.GetRequiredService<MongoRepositoryProvider<MongoPerson>>();

		protected MongoRepository<MongoPerson> MongoRepository => MongoRepositoryProvider.GetRepository(TenantId);

		protected IRepositoryProvider<MongoPerson> RepositoryProvider => serviceProvider.GetRequiredService<IRepositoryProvider<MongoPerson>>();

		protected IRepository<MongoPerson> Repository => RepositoryProvider.GetRepository(TenantId);

		protected IPageableRepository<MongoPerson> PageableRepository => Repository as IPageableRepository<MongoPerson>;

        protected IFilterableRepository<MongoPerson> FilterableRepository => Repository as IFilterableRepository<MongoPerson>;

        protected IRepositoryProvider<IPerson> FacadeRepositoryProvider => serviceProvider.GetRequiredService<IRepositoryProvider<IPerson>>();

		protected IRepository<IPerson> FacadeRepository => FacadeRepositoryProvider.GetRepository(TenantId);

		protected IPageableRepository<IPerson> FacadePageableRepository => FacadeRepository as IPageableRepository<IPerson>;

        protected IFilterableRepository<IPerson> FilterableFacadeRepository => FacadeRepository as IFilterableRepository<IPerson>;

        protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

		protected Faker<MongoPerson> PersonFaker { get; }

		protected MongoPerson GeneratePerson() => PersonFaker.Generate();

		protected IList<MongoPerson> GeneratePersons(int count) => PersonFaker.Generate(count);

		protected virtual void AddRepositoryProvider(IServiceCollection services) {
			services
				.AddMongoOptions(options => options
					.ConnectionString(mongo.ConnectionString)
					.Database("test_db")
					.Collection("Person", "persons")
					.WithTenantField())
				.AddMongoRepositoryProvider<MongoPerson>()
				.AddMongoFacadeRepositoryProvider<MongoPerson, IPerson>()
				.AddRepositoryController();
		}

		public virtual async Task InitializeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
			await controller.CreateTenantRepositoryAsync<MongoPerson>(TenantId);

			var repository = MongoRepositoryProvider.GetRepository(TenantId);
			
			//await repository.CreateAsync();

			await SeedAsync(repository);
		}

		public virtual async Task DisposeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
			await controller.DropTenantRepositoryAsync<MongoPerson>(TenantId);

			//var repository = MongoRepositoryProvider.GetRepository(TenantId);
			//await repository.DropAsync();
		}

		protected virtual Task SeedAsync(MongoRepository<MongoPerson> repository) {
			return Task.CompletedTask;
		}

		protected class MongoPerson : IMultiTenantDocument, IPerson {
			[BsonId]
			public ObjectId Id { get; set; }

			string? IMongoDocument.Id => Id.ToEntityId();

			string? IEntity.Id => Id.ToEntityId();

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public DateTime? BirthDate { get; set; }

			public string? Description { get; set; }

			public string TenantId { get; set; }
		}

		protected interface IPerson : IEntity {
			public string FirstName { get; }

			public string LastName { get; }

			public DateTime? BirthDate { get; }

			public string? Description { get; }
		}
	}
}
