using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
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

		public string ConnectionString => container.GetConnectionString();

		public string SetDatabase(string database) {
			var urlBuilder = new MongoUrlBuilder(ConnectionString);
			urlBuilder.DatabaseName = database;
			return urlBuilder.ToString();
		}

		public async Task DisposeAsync() {
			await container.StopAsync();
			await container.DisposeAsync();
		}

		public Task InitializeAsync() {
			return container.StartAsync();
		}
	}
}
