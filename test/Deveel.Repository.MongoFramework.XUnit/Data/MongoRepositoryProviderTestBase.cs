using System;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Finbuckle.MultiTenant;
using System.Configuration;
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
			services.AddMultiTenant<MongoTenantInfo>()
				.WithInMemoryStore(config => {
					config.Tenants.Add(new MongoTenantInfo {
						Id = TenantId,
						Identifier = "test",
						Name = "Test Tenant",
						DatabaseName = "test_db1",
						ConnectionString = mongo.ConnectionString
					}) ;
				});

			services
				.AddMongoDbTenantOptions(options => {
					options.DefaultDatabase = "testdb";
					options.DefaultConnectionString = ConnectionString;
				})
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

		[MultiTenant]
		protected class MongoPerson : IPerson, IHaveTenantId {
			[BsonId]
			public ObjectId Id { get; set; }

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
