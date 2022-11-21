using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson.Serialization.Attributes;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class RepositoryControllerTests : IClassFixture<MongoDbTestFixture> {
		private readonly IRepositoryController controller;

		public RepositoryControllerTests(MongoDbTestFixture mongo, ITestOutputHelper outputHelper) {
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
			await controller.DropRepositoryAsync<MongoPerson>();
		}

		[Fact]
		public async Task CreateAndDropSingleTenantRepository() {
			await controller.CreateTenantRepositoryAsync<MongoPerson>(TenantId);
			await controller.DropTenantRepositoryAsync<MongoPerson>(TenantId);
		}

		[Fact]
		public async Task CreateAndDropAllRepositories() {
			await controller.CreateAllRepositoriesAsync();
			await controller.DropAllRepositoriesAsync();
		}

		[MultiTenantDocument]
		class MongoPerson : IEntity {
			[BsonId]
			public string Id { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public string TenantId { get; set; }
		}
	}
}
