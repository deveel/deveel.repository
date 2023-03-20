using System;

using Deveel.Repository;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class RepositoryControllerTests : IClassFixture<MongoDbTestFixture> {
		private readonly IRepositoryController controller;
		private readonly MongoClient mongoClient;

		public RepositoryControllerTests(MongoDbTestFixture mongo, ITestOutputHelper outputHelper) {
			mongoClient = new MongoClient(MongoClientSettings.FromConnectionString(mongo.ConnectionString));

			var services = new ServiceCollection()
				.AddRepositoryController()
				.AddRepository<MongoRepository<MongoPerson>>()
				.AddMongoStoreOptions<MongoPerson>(options => options
					.ConnectionString(mongo.ConnectionString)
					.Database("test_db")
					.Collection("persons"))
				.AddMongoRepositoryProvider<MongoPerson>()
				.AddMongoOptions(options => options
					.ConnectionString(mongo.ConnectionString)
					.Database("test_db")
					.Collection("Person", "tenant_persons")
					.WithTenantField())
				.AddLogging(logging => logging.AddXUnit(outputHelper).SetMinimumLevel(LogLevel.Trace))
				.BuildServiceProvider();

			controller = services.GetRequiredService<IRepositoryController>();
		}

		public string TenantId { get; } = Guid.NewGuid().ToString();

		[Fact]
		public async Task CreateAndDropSingleRepository() {
			await controller.CreateRepositoryAsync<MongoPerson>();

			Assert.True(await mongoClient.GetDatabase("test_db").CollectionExistsAsync("persons"));

			await controller.DropRepositoryAsync<MongoPerson>();

			Assert.False(await mongoClient.GetDatabase("test_db").CollectionExistsAsync("persons"));
		}

		[Fact]
		public async Task CreateAndDropSingleTenantRepository() {
			await controller.CreateTenantRepositoryAsync<MongoPerson>(TenantId);

			Assert.True(await mongoClient.GetDatabase("test_db").CollectionExistsAsync("tenant_persons"));

			await controller.DropTenantRepositoryAsync<MongoPerson>(TenantId);

			Assert.False(await mongoClient.GetDatabase("test_db").CollectionExistsAsync("tenant_persons"));
		}

		[Fact]
		public async Task CreateAndDropAllRepositories() {
			await controller.CreateAllRepositoriesAsync();

			Assert.True(await mongoClient.GetDatabase("test_db").CollectionExistsAsync("persons"));

			await controller.DropAllRepositoriesAsync();

			Assert.False(await mongoClient.GetDatabase("test_db").CollectionExistsAsync("persons"));
		}

		[Fact]
		public async Task CreateAndDropTenantRepositories() {
			await controller.CreateTenantRepositoriesAsync(TenantId);
			Assert.True(await mongoClient.GetDatabase("test_db").CollectionExistsAsync("tenant_persons"));

			await controller.DropTenantRepositoriesAsync(TenantId);

			Assert.False(await mongoClient.GetDatabase("test_db").CollectionExistsAsync("tenant_persons"));
		}


		[MultiTenantDocument]
		class MongoPerson : IDataEntity {
			[BsonId]
			public string Id { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public string TenantId { get; set; }
		}
	}
}
