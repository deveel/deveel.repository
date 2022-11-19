using System;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

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
		}

		protected string TenantId { get; } = Guid.NewGuid().ToString("N");

		protected string ConnectionString => mongo.ConnectionString;

		protected MongoRepositoryProvider<MongoPerson> MongoRepositoryProvider => serviceProvider.GetRequiredService<MongoRepositoryProvider<MongoPerson>>();

		protected MongoRepository<MongoPerson> MongoRepository => MongoRepositoryProvider.GetRepository(TenantId);

		protected IRepositoryProvider<MongoPerson> RepositoryProvider => serviceProvider.GetRequiredService<IRepositoryProvider<MongoPerson>>();

		protected IRepository<MongoPerson> Repository => RepositoryProvider.GetRepository(TenantId);

		protected IRepositoryProvider<IPerson> FacadeRepositoryProvider => serviceProvider.GetRequiredService<IRepositoryProvider<IPerson>>();

		protected IRepository<IPerson> FacadeRepository => FacadeRepositoryProvider.GetRepository(TenantId);

		protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

		protected virtual void AddRepositoryProvider(IServiceCollection services) {
			services
				.AddMongoOptions(options => options
					.ConnectionString(mongo.ConnectionString)
					.Database("test_db")
					.Collection("Person", "persons")
					.WithTenantField())
				.AddMongoRepositoryProvider<MongoPerson>()
				.AddMongoFacadeRepositoryProvider<MongoPerson, IPerson>();
		}

		public async Task InitializeAsync() {
			var repository = MongoRepositoryProvider.GetRepository(TenantId);
			await repository.CreateAsync();

			await SeedAsync(repository);
		}

		public async Task DisposeAsync() {
			var repository = MongoRepositoryProvider.GetRepository(TenantId);

			await repository.DropAsync();
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

			public DateOnly? BirthDate { get; set; }

			public string? Description { get; set; }

			public string TenantId { get; set; }
		}

		protected interface IPerson : IEntity {
			public string FirstName { get; }

			public string LastName { get; }

			public DateOnly? BirthDate { get; }

			public string? Description { get; }
		}
	}
}
