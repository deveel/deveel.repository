
using Microsoft.Extensions.Logging.Abstractions;

using Mongo2Go;

using MongoDB.Driver;

namespace Deveel.Data {
    public class MongoFrameworkTestFixture : IDisposable {
		private MongoDbRunner mongo;

		public MongoFrameworkTestFixture() {
			mongo = MongoDbRunner.Start(logger: NullLogger.Instance, singleNodeReplSet: false);
		}

		public string ConnectionString => mongo.ConnectionString;

		public string SetDatabase(string database) {
			var urlBuilder = new MongoUrlBuilder(ConnectionString);
			urlBuilder.DatabaseName = database;
			return urlBuilder.ToString();
		}

		public void Dispose() {
			mongo?.Dispose();
		}

	}
}
