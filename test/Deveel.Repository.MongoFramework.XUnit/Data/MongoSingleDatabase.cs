
using EphemeralMongo;

using MongoDB.Driver;

namespace Deveel.Data {
	public class MongoSingleDatabase : IDisposable {
		private IMongoRunner mongo;

		public MongoSingleDatabase() {
			mongo = MongoRunner.Run(new MongoRunnerOptions {
				UseSingleNodeReplicaSet = true,
				KillMongoProcessesWhenCurrentProcessExits = true
			});
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
