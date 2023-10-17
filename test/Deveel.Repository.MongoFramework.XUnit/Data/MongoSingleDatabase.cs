using DotNet.Testcontainers.Containers;

using MongoDB.Driver;

using Testcontainers.MongoDb;

namespace Deveel.Data {
	public class MongoSingleDatabase : IAsyncLifetime {
		private readonly MongoDbContainer container;

		public MongoSingleDatabase() {
			container = new MongoDbBuilder()
				.WithUsername("")
				.WithPassword("")
				.WithPortBinding(27017)
				.Build();
		}

		public const string DatabaseName = "test_db";

		public string ConnectionString =>
				SetDatabase(container.GetConnectionString(), DatabaseName);

		private static string SetDatabase(string connectionString, string database) {
			var urlBuilder = new MongoUrlBuilder(connectionString);
			urlBuilder.DatabaseName = database;
			return urlBuilder.ToString();
		}

		public async Task DisposeAsync() {
			var client = new MongoClient(ConnectionString);
			await client.DropDatabaseAsync(DatabaseName);

			await container.StopAsync();
			while(container.State != TestcontainersStates.Exited) {
				await Task.Delay(100);
			}

			await container.DisposeAsync();
		}

		public Task InitializeAsync() {
			return container.StartAsync();
		}
	}
}
